using MediatR;

namespace EduVote.Application.VotingTargets.AddVotingTarget;

public sealed record AddVotingTargetCommand(Guid VotingId, Guid EducationUnitId)
    : IRequest<VotingTargetDetails>;
