using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.Application.Votings.GetResults;

public static class OpenAnswerResultsAccess
{
    public static bool CanViewAnswerTexts(string? role, Guid? callerUserId, Guid createdById)
    {
        if (role == Roles.Administrator)
            return true;

        if (role == Roles.Teacher
            && callerUserId.HasValue
            && callerUserId.Value == createdById)
            return true;

        return false;
    }
}
