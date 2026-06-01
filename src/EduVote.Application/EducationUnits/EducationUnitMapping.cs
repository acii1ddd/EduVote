using EduVote.DAL.Postgresql.Models;

namespace EduVote.Application.EducationUnits;

internal static class EducationUnitMapping
{
    public static EducationUnitDetails ToDetails(this EducationUnit unit) =>
        new(
            unit.Id,
            unit.Name,
            unit.Type.ToString(),
            unit.ParentId);
}
