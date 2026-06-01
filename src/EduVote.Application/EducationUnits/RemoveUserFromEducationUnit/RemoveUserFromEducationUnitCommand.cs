using MediatR;

namespace EduVote.Application.EducationUnits.RemoveUserFromEducationUnit;

public sealed record RemoveUserFromEducationUnitCommand(
    Guid UserId,
    Guid EducationUnitId) : IRequest;
