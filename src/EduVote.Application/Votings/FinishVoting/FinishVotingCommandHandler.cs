using EduVote.Application.Votings.Services;
using EduVote.DAL.Postgresql.Models;
using Microsoft.Extensions.Logging;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.FinishVoting;

public sealed class FinishVotingCommandHandler(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository,
    IVotingResultRepository votingResultRepository,
    IBlockchainRecordRepository blockchainRecordRepository,
    IBlockchainResultWriter blockchainResultWriter,
    VotingResultCalculatorService votingResultCalculatorService,
    VotingStatusService votingStatusService,
    ILogger<FinishVotingCommandHandler> logger)
    : IRequestHandler<FinishVotingCommand, FinishVotingResult>
{
    public async Task<FinishVotingResult> Handle(
        FinishVotingCommand request,
        CancellationToken cancellationToken)
    {
        var voting = await votingStatusService.GetVotingOrThrowAsync(request.VotingId, cancellationToken);

        // Skip votings that cannot be finalized
        if (voting.Status is DbVotingStatus.Finished or DbVotingStatus.Draft or DbVotingStatus.PendingApproval)
            return new FinishVotingResult(request.VotingId, null, null);

        VotingStatusService.ChangeStatus(
            voting,
            DbVotingStatus.Finished,
            [DbVotingStatus.Active, DbVotingStatus.Paused]);

        await votingRepository.SaveChangesAsync(cancellationToken);

        var existingResult = await votingResultRepository
            .GetByVotingIdAsync(request.VotingId, cancellationToken);

        if (existingResult is not null)
        {
            var existingRecord = await blockchainRecordRepository
                .GetByVotingResultIdAsync(existingResult.Id, cancellationToken);

            if (existingRecord is null)
                return new FinishVotingResult(request.VotingId, null, null);

            return new FinishVotingResult(
                request.VotingId,
                existingRecord.TransactionHash,
                $"https://sepolia.etherscan.io/tx/{existingRecord.TransactionHash}");
        }

        var candidates = await candidateRepository
            .GetByVotingIdAsync(request.VotingId, cancellationToken);

        var votingResult = await votingResultCalculatorService.CalculateVotingResultAsync(
            voting,
            candidates,
            cancellationToken);

        await votingResultRepository.CreateAsync(votingResult, cancellationToken);

        try
        {
            var (txHash, blockNumber) = await blockchainResultWriter
                .WriteResultHashAsync(votingResult.ResultHash);

            await blockchainRecordRepository.CreateAsync(new BlockchainRecord
            {
                Id = Guid.NewGuid(),
                VotingResultId = votingResult.Id,
                VotingId = request.VotingId,
                TransactionHash = txHash,
                VotesHash = votingResult.ResultHash,
                BlockNumber = blockNumber,
                Network = "sepolia",
                Status = "confirmed",
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            return new FinishVotingResult(
                request.VotingId,
                txHash,
                $"https://sepolia.etherscan.io/tx/{txHash}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Blockchain write failed for voting {VotingId}", request.VotingId);
            return new FinishVotingResult(request.VotingId, null, null);
        }
    }
}
