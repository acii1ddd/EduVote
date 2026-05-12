using EduVote.DAL.Postgresql.Models;

namespace EduVote.API.Mappers;

public static class EducationUnitMapper
{
    public static EducationUnitResponse MapToResponse(
        this EducationUnit entity)
    {
        return new EducationUnitResponse
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Type = entity.Type.ToString(),
            ParentId = entity.ParentId?.ToString() ?? string.Empty
        };
    }

    public static IEnumerable<EducationUnitResponse>
        MapToResponseList(
            this IEnumerable<EducationUnit> entities)
    {
        return entities.Select(x => x.MapToResponse());
    }
}