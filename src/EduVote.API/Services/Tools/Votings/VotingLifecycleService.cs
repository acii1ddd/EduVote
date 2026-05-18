using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.API.Services.Tools.Votings;

public class VotingLifecycleService(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository,
    IVotingResultRepository votingResultRepository,
    VotingResultCalculatorService votingResultCalculatorService,
    BlockchainService blockchainService,
    IBlockchainRecordRepository blockchainRecordRepository,
    ILogger<VotingLifecycleService> logger)
{
    public async Task<(string? TxHash, string? EtherscanUrl)> FinalizeVotingAsync(Guid votingId,
        CancellationToken cancellationToken = default)
    {
        var voting = await GetVotingOrThrowAsync(votingId, cancellationToken);

        // Skip votings that cannot be finalized
        if (voting.Status is DbVotingStatus.Finished or DbVotingStatus.Draft or DbVotingStatus.PendingApproval)
            return (null, null);

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
            var existingRecord = await blockchainRecordRepository
                .GetByVotingResultIdAsync(existingResult.Id, cancellationToken);

            if (existingRecord is not null)
            {
                var existingUrl = $"https://sepolia.etherscan.io/tx/{existingRecord.TransactionHash}";
                return (existingRecord.TransactionHash, existingUrl);
            }

            return (null, null);
        }

        // Calculate new results
        var candidates = await candidateRepository
            .GetByVotingIdAsync(votingId, cancellationToken);

        var votingResult = await votingResultCalculatorService.CalculateVotingResultAsync(
            voting,
            candidates,
            cancellationToken
        );

        await votingResultRepository
            .CreateAsync(votingResult, cancellationToken);

        string? txHash = null;
        string? etherscanUrl = null;

        try
        {
            (txHash, var blockNumber) = await blockchainService
                .WriteResultHashAsync(votingResult.ResultHash);
            
            etherscanUrl = $"https://sepolia.etherscan.io/tx/{txHash}";

            logger.LogInformation(
                "[FinalizeVoting] Voting {VotingId} anchored. TxHash: {TxHash}, Etherscan: {EtherscanUrl}",
                votingId, txHash, etherscanUrl
            );

            await blockchainRecordRepository.CreateAsync(new BlockchainRecord
            {
                Id = Guid.NewGuid(),
                VotingResultId = votingResult.Id,
                VotingId = votingId,
                TransactionHash = txHash,
                VotesHash = votingResult.ResultHash,
                BlockNumber = blockNumber.ToString(),
                Network = "sepolia",
                Status = "confirmed",
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FinalizeVoting] Blockchain write failed for voting {VotingId}", votingId);
        }

        return (txHash, etherscanUrl);
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