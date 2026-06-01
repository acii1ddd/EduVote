using MediatR;

namespace EduVote.Application.EducationUnits.AssignUserToEducationUnit;

public sealed record AssignUserToEducationUnitCommand(
    Guid UserId,
    Guid EducationUnitId) : IRequest;
