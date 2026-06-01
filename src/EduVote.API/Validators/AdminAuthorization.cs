using System.Security.Claims;
using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.API.Validators;

internal static class AdminAuthorization
{
    public static void EnsureAdministrator(ServerCallContext context)
    {
        var role = context.GetHttpContext().User.FindFirst(ClaimTypes.Role)?.Value;
        if (role != Roles.Administrator)
        {
            throw new RpcException(new Status(
                StatusCode.PermissionDenied,
                "Analytics is available to administrators only."));
        }
    }
}
