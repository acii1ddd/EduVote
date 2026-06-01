using MediatR;

namespace EduVote.Application.VotingTargets.DeleteVotingTarget;

public sealed record DeleteVotingTargetCommand(Guid VotingId, Guid EducationUnitId) : IRequest;
