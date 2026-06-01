using EduVote.Application.Common;
using EduVote.Application.Votings.Services;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.GetVerificationData;

public sealed class GetVerificationDataQueryHandler(
    VotingStatusService votingStatusService,
    IVotingResultRepository votingResultRepository,
    IVoteRepository voteRepository)
    : IRequestHandler<GetVerificationDataQuery, GetVerificationDataResult>
{
    public async Task<GetVerificationDataResult> Handle(
        GetVerificationDataQuery request,
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

        var votes = (await voteRepository
            .GetByVotingIdAsync(request.VotingId, cancellationToken)).ToList();

        return new GetVerificationDataResult(
            request.VotingId,
            existingResult.ResultHash,
            votes.Select(v => v.VoteHash).ToList(),
            votes.Count);
    }
}
