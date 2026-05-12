using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.API.Mappers;

public static class RoleMapper
{
    private static readonly TypeAdapterConfig Config = new();
    
    static RoleMapper()
    {
        Config.NewConfig<RoleResponse, Role>()
            .Map(dest => dest.Name, src => src.Name);
    }
    
    public static RoleResponse MapToResponse(this Role source)
    {
        return source.Adapt<RoleResponse>();
    }
    
    public static IEnumerable<RoleResponse> MapToResponseList(this IEnumerable<Role> source)
    {
        return source.Adapt<IEnumerable<RoleResponse>>();
    }
}