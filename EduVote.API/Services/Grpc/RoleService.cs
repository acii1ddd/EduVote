using EduVote.API.Mappers;
using EduVote.DAL.Postgresql.Repositories;

namespace EduVote.API.Services.Grpc;

public class RoleService(IRoleRepository roleRepository) 
    : RolesService.RolesServiceBase
{
    public override async Task<GetRolesResponse> GetRoles(
        Empty request, ServerCallContext context)
    {
        var roles = await roleRepository.GetAllAsync();

        var response = new GetRolesResponse();
        response.Roles.AddRange(roles.MapToResponseList());
        
        return response;
    }
}