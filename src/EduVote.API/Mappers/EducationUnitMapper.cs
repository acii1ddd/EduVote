using EduVote.Application.EducationUnits;

namespace EduVote.API.Mappers;

public static class EducationUnitMapper
{
    public static EducationUnitResponse MapToResponse(this EducationUnitDetails source) =>
        new()
        {
            Id = source.Id.ToString(),
            Name = source.Name,
            Type = source.Type,
            ParentId = source.ParentId?.ToString() ?? string.Empty
        };

    public static IEnumerable<EducationUnitResponse> MapToResponseList(
        this IEnumerable<EducationUnitDetails> source) =>
        source.Select(MapToResponse);
}
