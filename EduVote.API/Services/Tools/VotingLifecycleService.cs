using EduVote.DAL.Postgresql.Repositories;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.API.Services.Tools;

public class VotingLifecycleService(
    IVotingRepository votingRepository, 
    ICandidateRepository candidateRepository, 
    IVotingResultRepository votingResultRepository,
    VotingResultCalculatorService votingResultCalculatorService)
{
    public async Task FinalizeVotingAsync(Guid votingId, 
        CancellationToken cancellationToken = default)
    {
        var voting = await GetVotingOrThrowAsync(votingId, cancellationToken);
        
        // Skip votings that cannot be finalized
        if (voting.Status is DbVotingStatus.Finished or DbVotingStatus.Draft)
            return;
        
        ChangeStatus(
            voting,
            DbVotingStatus.Finished,
            [DbVotingStatus.Active, DbVotingStatus.Paused]
        );
        
        await votingRepository
            .SaveChangesAsync(cancellationToken);
        
        var existingResult = await votingResultRepository
            .GetByVotingIdAsync(votingId, cancellationToken);
        
        if (existingResult is not null)
        {
            return;
        }
        
        // Calculate new results
        var candidates = await candidateRepository
            .GetByVotingIdAsync(votingId, cancellationToken);
        
        var votingResult = await votingResultCalculatorService.CalculateVotingResultAsync(
            voting, 
            candidates, 
            cancellationToken
        );

        // Save results to database
        await votingResultRepository
            .CreateAsync(votingResult, cancellationToken);
    }
    
    public async Task ChangeStatusAsync(
        Guid votingId,
        DbVotingStatus newStatus,
        IReadOnlyCollection<DbVotingStatus> allowedCurrentStatuses,
        CancellationToken cancellationToken)
    {
        var voting = await GetVotingOrThrowAsync(votingId, cancellationToken);
        
        ChangeStatus(voting, newStatus, allowedCurrentStatuses);
        
        await votingRepository
            .SaveChangesAsync(cancellationToken);
    }
    
    private static void ChangeStatus(
        DbVoting voting,
        DbVotingStatus newStatus,
        IReadOnlyCollection<DbVotingStatus> allowedCurrentStatuses)
    {
        if (!allowedCurrentStatuses.Contains(voting.Status))
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"Voting {voting.Id} cannot be moved from {voting.Status} to {newStatus}."));
        }

        voting.Status = newStatus;
    }
    
    private async Task<DbVoting> GetVotingOrThrowAsync(
        Guid votingId,
        CancellationToken cancellationToken)
    {
        var existingVoting = await votingRepository
            .GetByIdAsync(votingId, cancellationToken);

        if (existingVoting is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Voting with id {votingId} was not found."));
        }

        return existingVoting;
    }
}