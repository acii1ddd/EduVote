using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.GetVotings;

public sealed class GetVotingsQueryHandler(IVotingRepository votingRepository)
    : IRequestHandler<GetVotingsQuery, IReadOnlyList<VotingDetails>>
{
    public async Task<IReadOnlyList<VotingDetails>> Handle(
        GetVotingsQuery request,
        CancellationToken cancellationToken)
    {
        var votings = await votingRepository.GetAllAsync(cancellationToken);
        return votings.Select(v => v.ToDetails()).ToList();
    }
}
