using EduVote.DAL.Postgresql.Models;

namespace EduVote.Application.VotingTargets;

internal static class VotingTargetMapping
{
    public static VotingTargetDetails ToDetails(this VotingTarget target) =>
        new(target.VotingId, target.EducationUnitId);
}
