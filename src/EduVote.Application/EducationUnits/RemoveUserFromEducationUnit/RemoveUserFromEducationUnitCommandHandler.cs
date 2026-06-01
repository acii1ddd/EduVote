using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.EducationUnits.RemoveUserFromEducationUnit;

public sealed class RemoveUserFromEducationUnitCommandHandler(
    IUserEducationUnitRepository userEducationUnitRepository)
    : IRequestHandler<RemoveUserFromEducationUnitCommand>
{
    public async Task Handle(
        RemoveUserFromEducationUnitCommand request,
        CancellationToken cancellationToken)
    {
        await userEducationUnitRepository.RemoveAsync(
            request.UserId,
            request.EducationUnitId,
            cancellationToken);
    }
}
