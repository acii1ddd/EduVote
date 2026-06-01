using EduVote.Application.RolesCatalog;

namespace EduVote.API.Mappers;

public static class RoleMapper
{
    public static RoleResponse MapToResponse(this RoleDetails source) =>
        new() { Name = source.Name };

    public static IEnumerable<RoleResponse> MapToResponseList(this IEnumerable<RoleDetails> source) =>
        source.Select(MapToResponse);
}
