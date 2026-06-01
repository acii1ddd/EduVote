using EduVote.Application.Common;
using EduVote.Application.Votings.Services;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.GetMyVote;

public sealed class GetMyVoteQueryHandler(
    VotingStatusService votingStatusService,
    IVoteRepository voteRepository,
    ICandidateRepository candidateRepository,
    IVoteHashService voteHashService)
    : IRequestHandler<GetMyVoteQuery, GetMyVoteResult>
{
    public async Task<GetMyVoteResult> Handle(
        GetMyVoteQuery request,
        CancellationToken cancellationToken)
    {
        var voting = await votingStatusService
            .GetVotingOrThrowAsync(request.VotingId, cancellationToken);

        var vote = await voteRepository
            .GetUserVoteAsync(request.VotingId, request.UserId, cancellationToken);

        if (vote is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                "You have not voted in this voting.");
        }

        if (vote.UserId != request.UserId)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.PermissionDenied,
                "This vote does not belong to the current user.");
        }

        var candidates = (await candidateRepository
            .GetByVotingIdAsync(request.VotingId, cancellationToken)).ToList();

        var hashInput = voteHashService.BuildHashInput(vote, voting.Type);
        var voteDataJson = MyVoteDataJsonBuilder.Build(vote, voting.Type, candidates);

        return new GetMyVoteResult(
            vote.Id,
            vote.VoteHash,
            vote.VoteSalt,
            hashInput,
            voteDataJson);
    }
}
