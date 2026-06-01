using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.EducationUnits.GetEducationUnits;

public sealed class GetEducationUnitsQueryHandler(IEducationUnitRepository educationUnitRepository)
    : IRequestHandler<GetEducationUnitsQuery, IReadOnlyList<EducationUnitDetails>>
{
    public async Task<IReadOnlyList<EducationUnitDetails>> Handle(
        GetEducationUnitsQuery request,
        CancellationToken cancellationToken)
    {
        var units = await educationUnitRepository
            .GetAllAsync(cancellationToken);

        return units.Select(u => u.ToDetails()).ToList();
    }
}
