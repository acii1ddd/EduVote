using EduVote.Application.Common;
using EduVote.Application.Votings.Services;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

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

        if (voting.Type == DbVotingType.OpenAnswer && !request.CallerUserId.HasValue)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.Unauthenticated,
                "Authentication is required to view voting results.");
        }

        var openAnswerTextsVisible = voting.Type != DbVotingType.OpenAnswer
            || OpenAnswerResultsAccess.CanViewAnswerTexts(
                request.CallerRole,
                request.CallerUserId,
                voting.CreatedById);

        var resultData = existingResult.ResultData;
        if (voting.Type == DbVotingType.OpenAnswer
            && !openAnswerTextsVisible
            && !string.IsNullOrEmpty(resultData))
        {
            resultData = OpenAnswerResultDataRedactor.RedactAnswerTexts(resultData);
        }

        var blockchainRecord = await blockchainRecordRepository
            .GetByVotingResultIdAsync(existingResult.Id, cancellationToken);

        return new GetResultsResult(
            existingResult.VotingId,
            existingResult.ResultHash,
            existingResult.CalculatedAt,
            existingResult.TotalVotes,
            resultData,
            blockchainRecord?.TransactionHash,
            blockchainRecord is null
                ? null
                : $"https://sepolia.etherscan.io/tx/{blockchainRecord.TransactionHash}",
            openAnswerTextsVisible);
    }
}
