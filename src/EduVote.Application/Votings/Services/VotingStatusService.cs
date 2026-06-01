using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.Services;

public sealed class VotingStatusService(IVotingRepository votingRepository)
{
    public async Task ChangeStatusAsync(
        Guid votingId,
        DbVotingStatus newStatus,
        IReadOnlyCollection<DbVotingStatus> allowedCurrentStatuses,
        CancellationToken cancellationToken)
    {
        var voting = await GetVotingOrThrowAsync(votingId, cancellationToken);
        ChangeStatus(voting, newStatus, allowedCurrentStatuses);

        await votingRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<DbVoting> GetVotingOrThrowAsync(
        Guid votingId,
        CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(votingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id {votingId} was not found.");
        }

        return voting;
    }

    public static void ChangeStatus(
        DbVoting voting,
        DbVotingStatus newStatus,
        IReadOnlyCollection<DbVotingStatus> allowedCurrentStatuses)
    {
        if (!allowedCurrentStatuses.Contains(voting.Status))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.FailedPrecondition,
                $"Voting {voting.Id} cannot be moved from {voting.Status} to {newStatus}.");
        }

        voting.Status = newStatus;
    }
}
