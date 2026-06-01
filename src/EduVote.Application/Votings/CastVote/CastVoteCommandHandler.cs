using EduVote.Application.Common;
using EduVote.Application.Votings.Services;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Application.Votings.CastVote;

public sealed class CastVoteCommandHandler(
    IVotingRepository votingRepository,
    IVoteRepository voteRepository,
    IUserRepository userRepository,
    ICandidateRepository candidateRepository,
    IVotingTargetRepository votingTargetRepository,
    IEducationUnitRepository educationUnitRepository,
    IVoteHashService voteHashService)
    : IRequestHandler<CastVoteCommand, CastVoteResult>
{
    public async Task<CastVoteResult> Handle(
        CastVoteCommand command,
        CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(command.VotingId, cancellationToken);

        if (voting is null)
            throw new ApplicationErrorException(ApplicationErrorType.NotFound, "Voting not found.");

        if (voting.Status == DbVotingStatus.Finished)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.FailedPrecondition,
                "Voting is already finished.");
        }

        var user = await userRepository.GetByIdWithEducationUnitsAsync(command.UserId, cancellationToken);

        if (user is null)
            throw new ApplicationErrorException(ApplicationErrorType.NotFound, "User not found.");

        await EnsureUserHasAccessAsync(command, user, cancellationToken);

        var existingVote = await voteRepository.GetUserVoteAsync(
            command.VotingId,
            command.UserId,
            cancellationToken);

        if (existingVote is not null && !voting.AllowVoteChange)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.AlreadyExists,
                "User has already voted in this voting and vote change is not allowed.");
        }

        var candidates = (await candidateRepository
                .GetByVotingIdAsync(command.VotingId, cancellationToken))
            .ToList();

        CastVoteValidator.ValidateVote(command, voting, candidates);

        var salt = Guid.NewGuid().ToString("N");
        var voteHash = voteHashService.GenerateVoteHash(
            command.VotingId,
            command.UserId,
            voting.Type,
            salt,
            command);

        if (existingVote is not null)
        {
            UpdateExistingVote(existingVote, command, voteHash, salt, voting.Type);
            await voteRepository.SaveChangesAsync(cancellationToken);

            return existingVote.ToCastVoteResult();
        }

        var newVote = CreateNewVote(command, voteHash, salt, voting.Type);
        var createdVote = await voteRepository.CreateAsync(newVote, cancellationToken);

        return createdVote.ToCastVoteResult();
    }

    private async Task EnsureUserHasAccessAsync(
        CastVoteCommand command,
        User user,
        CancellationToken cancellationToken)
    {
        var votingTargets = await votingTargetRepository
            .GetByVotingIdAsync(command.VotingId, cancellationToken);

        var targetEducationUnitIds = votingTargets
            .Select(vt => vt.EducationUnitId)
            .ToList();

        if (targetEducationUnitIds.Count == 0)
            return;

        var userEducationUnitIds = user.UserEducationUnits
            .Select(ueu => ueu.EducationUnitId)
            .ToList();

        var userAllUnitsWithAncestorIds = (
                await educationUnitRepository.GetAllParentIdsAsync(userEducationUnitIds, cancellationToken))
            .ToList();

        var hasAccess = targetEducationUnitIds
            .Any(targetId => userAllUnitsWithAncestorIds.Contains(targetId));

        if (!hasAccess)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.PermissionDenied,
                "User does not have access to vote in this voting.");
        }
    }

    private void UpdateExistingVote(
        Vote vote,
        CastVoteCommand command,
        string voteHash,
        string salt,
        DbVotingType votingType)
    {
        vote.VoteHash = voteHash;
        vote.VoteSalt = salt;

        PopulateVoteData(vote, command, votingType);
    }

    private Vote CreateNewVote(
        CastVoteCommand command,
        string voteHash,
        string salt,
        DbVotingType votingType)
    {
        var vote = new Vote
        {
            Id = Guid.NewGuid(),
            VotingId = command.VotingId,
            UserId = command.UserId,
            VoteHash = voteHash,
            VoteSalt = salt,
        };

        PopulateVoteData(vote, command, votingType);
        return vote;
    }

    private void PopulateVoteData(
        Vote vote,
        CastVoteCommand command,
        DbVotingType votingType)
    {
        vote.CandidateId = null;
        vote.SelectedCandidateIds = null;
        vote.RatingAnswers = null;
        vote.TextAnswer = null;

        switch (votingType)
        {
            case DbVotingType.SingleChoice:
                vote.CandidateId = Guid.Parse(command.SelectedCandidateId);
                break;
            case DbVotingType.MultipleChoice:
                vote.SelectedCandidateIds = voteHashService.GetCanonicalVoteDataJson(votingType, command);
                break;
            case DbVotingType.Rating:
                vote.RatingAnswers = voteHashService.GetCanonicalVoteDataJson(votingType, command);
                break;
            case DbVotingType.OpenAnswer:
                vote.TextAnswer = command.TextAnswer;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(votingType), votingType, "Unknown vote type");
        }
    }
}

file static class VoteExtensions
{
    public static CastVoteResult ToCastVoteResult(this Vote vote) =>
        new(vote.Id, vote.VoteHash, vote.CreatedAt, vote.VoteSalt);
}
