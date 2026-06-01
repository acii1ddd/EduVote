using MediatR;

namespace EduVote.Application.EducationUnits.GetEducationUnits;

public sealed record GetEducationUnitsQuery 
    : IRequest<IReadOnlyList<EducationUnitDetails>>;
