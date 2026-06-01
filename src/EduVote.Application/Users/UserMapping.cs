using EduVote.DAL.Postgresql.Models;

namespace EduVote.Application.Users;

internal static class UserMapping
{
    public static UserDetails ToDetails(this User user)
    {
        var firstUnit = user.UserEducationUnits.FirstOrDefault();

        return new UserDetails(
            user.Id,
            user.Email,
            user.Name,
            user.UserRole.Name,
            firstUnit?.EducationUnitId,
            firstUnit?.EducationUnit?.Name,
            user.CreatedAt);
    }
}
