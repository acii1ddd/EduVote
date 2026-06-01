using EduVote.Application.Common;
using EduVote.Application.Votings.Services;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.GetResults;

public sealed class GetResultsQueryHandler(
    VotingStatusService votingStatusService,
    IVotingResultRepository votingResultRepository,
    IBlockchainRecordRepository blockchainRecordRepository)
    : IRequestHandler<GetResultsQuery, GetResultsResult>
{
    public async Task<GetResultsResult> Handle(
        GetResultsQuery request,
        CancellationToken cancellationToken)
    {
        var voting = await votingStatusService
            .GetVotingOrThrowAsync(request.VotingId, cancellationToken);

        if (voting.Status != DbVotingStatus.Finished)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.FailedPrecondition,
                "Voting is not finished yet.");
        }

        var existingResult = await votingResultRepository
            .GetByVotingIdAsync(request.VotingId, cancellationToken);

        if (existingResult is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.Unavailable,
                "Results will be available in the next minute.");
        }

        var blockchainRecord = await blockchainRecordRepository
            .GetByVotingResultIdAsync(existingResult.Id, cancellationToken);

        return new GetResultsResult(
            existingResult.VotingId,
            existingResult.ResultHash,
            existingResult.CalculatedAt,
            existingResult.TotalVotes,
            existingResult.ResultData,
            blockchainRecord?.TransactionHash,
            blockchainRecord is null
                ? null
                : $"https://sepolia.etherscan.io/tx/{blockchainRecord.TransactionHash}");
    }
}
