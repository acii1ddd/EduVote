using EduVote.Application.VotingTargets;

namespace EduVote.API.Mappers;

public static class VotingTargetMapper
{
    public static VotingTargetResponse MapToResponse(this VotingTargetDetails source) =>
        new()
        {
            VotingId = source.VotingId.ToString(),
            EducationUnitId = source.EducationUnitId.ToString()
        };

    public static IEnumerable<VotingTargetResponse> MapToResponseList(
        this IEnumerable<VotingTargetDetails> source) =>
        source.Select(MapToResponse);
}
