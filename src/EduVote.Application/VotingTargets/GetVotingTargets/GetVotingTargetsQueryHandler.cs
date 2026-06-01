using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.VotingTargets.GetVotingTargets;

public sealed class GetVotingTargetsQueryHandler(
    IVotingRepository votingRepository,
    IVotingTargetRepository votingTargetRepository)
    : IRequestHandler<GetVotingTargetsQuery, IReadOnlyList<VotingTargetDetails>>
{
    public async Task<IReadOnlyList<VotingTargetDetails>> Handle(
        GetVotingTargetsQuery request,
        CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.VotingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id '{request.VotingId}' was not found.");
        }

        var targets = await votingTargetRepository.GetByVotingIdAsync(
            request.VotingId,
            cancellationToken);

        return targets.Select(t => t.ToDetails()).ToList();
    }
}
