using EduVote.Application.Votings;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.GetVotingsCreatedByUser;

public sealed class GetVotingsCreatedByUserQueryHandler(IVotingRepository votingRepository)
    : IRequestHandler<GetVotingsCreatedByUserQuery, IReadOnlyList<VotingDetails>>
{
    public async Task<IReadOnlyList<VotingDetails>> Handle(
        GetVotingsCreatedByUserQuery request,
        CancellationToken cancellationToken)
    {
        var votings = await votingRepository
            .GetByCreatedByAsync(request.UserId, cancellationToken);

        return votings.Select(v => v.ToDetails()).ToList();
    }
}
