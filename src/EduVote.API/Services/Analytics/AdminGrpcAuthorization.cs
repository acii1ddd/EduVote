using System.Security.Claims;
using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.API.Services.Analytics;

internal static class AdminGrpcAuthorization
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
