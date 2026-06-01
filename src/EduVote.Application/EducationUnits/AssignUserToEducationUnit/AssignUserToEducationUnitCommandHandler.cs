using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.EducationUnits.AssignUserToEducationUnit;

public sealed class AssignUserToEducationUnitCommandHandler(
    IUserEducationUnitRepository userEducationUnitRepository)
    : IRequestHandler<AssignUserToEducationUnitCommand>
{
    public Task Handle(
        AssignUserToEducationUnitCommand request,
        CancellationToken cancellationToken) =>
        userEducationUnitRepository.ReplaceForUserAsync(
            request.UserId,
            request.EducationUnitId,
            cancellationToken);
}
