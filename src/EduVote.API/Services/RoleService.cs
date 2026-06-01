using EduVote.API.Mappers;
using EduVote.Application.RolesCatalog.GetRoles;
using MediatR;

namespace EduVote.API.Services;

public class RoleService(ISender sender) : RolesService.RolesServiceBase
{
    public override async Task<GetRolesResponse> GetRoles(
        Empty request,
        ServerCallContext context)
    {
        var roles = await sender
            .Send(new GetRolesQuery(), context.CancellationToken);

        var response = new GetRolesResponse();
        response.Roles.AddRange(roles.MapToResponseList());

        return response;
    }
}
