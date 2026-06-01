using MediatR;

namespace EduVote.Application.VotingTargets.GetVotingTargets;

public sealed record GetVotingTargetsQuery(Guid VotingId)
    : IRequest<IReadOnlyList<VotingTargetDetails>>;
