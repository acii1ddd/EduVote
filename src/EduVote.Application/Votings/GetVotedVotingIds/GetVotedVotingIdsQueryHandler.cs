using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.GetVotedVotingIds;

public sealed class GetVotedVotingIdsQueryHandler(IVoteRepository voteRepository)
    : IRequestHandler<GetVotedVotingIdsQuery, IReadOnlyList<Guid>>
{
    public async Task<IReadOnlyList<Guid>> Handle(
        GetVotedVotingIdsQuery request,
        CancellationToken cancellationToken)
    {
        var ids = await voteRepository
            .GetVotedVotingIdsAsync(request.UserId, cancellationToken);

        return ids.ToList();
    }
}
