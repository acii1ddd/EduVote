using EduVote.Application.Common;
using EduVote.Application.Users.Services;
using EduVote.Application.Votings.CastVote;
using EduVote.Application.Votings.Services;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Application.Votings.SeedDemoVotes;

public sealed class SeedDemoVotesCommandHandler(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository,
    IUserRepository userRepository,
    IVoteRepository voteRepository,
    IVotingTargetRepository votingTargetRepository,
    IUserEducationUnitRepository userEducationUnitRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IVoteHashService voteHashService)
    : IRequestHandler<SeedDemoVotesCommand, SeedDemoVotesResult>
{
    private static readonly double[] DefaultVoteWeights = [0.34, 0.24, 0.18, 0.14, 0.10];

    public async Task<SeedDemoVotesResult> Handle(
        SeedDemoVotesCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VotingTitleSubstring))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "Voting title substring is required.");
        }

        if (request.VoteCount < 1)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "Vote count must be at least 1.");
        }

        var voting = await votingRepository.FindByTitleContainingAsync(
            request.VotingTitleSubstring,
            cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with title containing '{request.VotingTitleSubstring}' was not found.");
        }

        if (voting.Type != DbVotingType.SingleChoice)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.FailedPrecondition,
                "Demo vote seeding is supported for SingleChoice votings only.");
        }

        var candidates = (await candidateRepository
                .GetByVotingIdAsync(voting.Id, cancellationToken))
            .OrderBy(c => c.Name)
            .ToList();

        if (candidates.Count == 0)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.FailedPrecondition,
                "Voting has no candidates.");
        }

        var studentRole = await roleRepository.GetByNameAsync(Roles.Student, cancellationToken);

        if (studentRole is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Role '{Roles.Student}' was not found.");
        }

        var targetUnitId = await ResolveTargetEducationUnitIdAsync(voting.Id, cancellationToken);

        await voteRepository.DeleteByVotingIdAsync(voting.Id, cancellationToken);

        await userRepository.DeleteByEmailPrefixAsync(
            $"demo-{voting.Id:N}-",
            cancellationToken);

        var passwordHash = passwordHasher.Hash("demo-vote-password");
        var candidateAssignments = BuildCandidateAssignments(candidates, request.VoteCount);
        var votes = new List<Vote>(request.VoteCount);

        for (var i = 0; i < request.VoteCount; i++)
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = $"demo-{voting.Id:N}-{i + 1}@eduvote.local",
                Name = $"Demo voter {i + 1}",
                RoleId = studentRole.Id,
                PasswordHash = passwordHash
            };

            await userRepository.AddAsync(user, cancellationToken);

            if (targetUnitId.HasValue)
            {
                await userEducationUnitRepository.ReplaceForUserAsync(
                    userId,
                    targetUnitId.Value,
                    cancellationToken);
            }

            var candidateId = candidateAssignments[i];
            var salt = Guid.NewGuid().ToString("N");
            var castCommand = new CastVoteCommand(
                voting.Id,
                userId,
                candidateId.ToString(),
                [],
                new Dictionary<string, int>(),
                string.Empty);

            var voteHash = voteHashService.GenerateVoteHash(
                voting.Id,
                userId,
                voting.Type,
                salt,
                castCommand);

            votes.Add(new Vote
            {
                Id = Guid.NewGuid(),
                VotingId = voting.Id,
                UserId = userId,
                CandidateId = candidateId,
                VoteSalt = salt,
                VoteHash = voteHash
            });
        }

        await voteRepository.AddRangeAsync(votes, cancellationToken);

        return new SeedDemoVotesResult(
            voting.Id,
            voting.Title,
            request.VoteCount,
            request.VoteCount);
    }

    private async Task<Guid?> ResolveTargetEducationUnitIdAsync(
        Guid votingId,
        CancellationToken cancellationToken)
    {
        var targets = await votingTargetRepository.GetByVotingIdAsync(votingId, cancellationToken);
        var first = targets.FirstOrDefault();

        return first?.EducationUnitId;
    }

    private static List<Guid> BuildCandidateAssignments(List<Candidate> candidates, int voteCount)
    {
        var weights = BuildWeights(candidates.Count);
        var counts = new int[candidates.Count];
        var assigned = 0;

        for (var i = 0; i < candidates.Count - 1; i++)
        {
            counts[i] = (int)Math.Round(voteCount * weights[i]);
            assigned += counts[i];
        }

        counts[^1] = voteCount - assigned;

        var assignments = new List<Guid>(voteCount);

        for (var i = 0; i < candidates.Count; i++)
        {
            for (var j = 0; j < counts[i]; j++)
            {
                assignments.Add(candidates[i].Id);
            }
        }

        return assignments;
    }

    private static double[] BuildWeights(int candidateCount)
    {
        if (candidateCount <= DefaultVoteWeights.Length)
        {
            var slice = DefaultVoteWeights.Take(candidateCount).ToArray();
            var sum = slice.Sum();

            return slice.Select(w => w / sum).ToArray();
        }

        var even = 1.0 / candidateCount;

        return Enumerable.Repeat(even, candidateCount).ToArray();
    }
}
