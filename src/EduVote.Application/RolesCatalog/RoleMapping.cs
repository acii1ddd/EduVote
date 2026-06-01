using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.Application.RolesCatalog;

internal static class RoleMapping
{
    public static RoleDetails ToDetails(this Role role) => new(role.Name);
}
