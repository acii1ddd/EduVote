using DbVotingTarget = EduVote.DAL.Postgresql.Models.VotingTarget;

namespace EduVote.API.Mappers;

public static class VotingTargetMapper
{
    private static readonly TypeAdapterConfig Config = new();

    static VotingTargetMapper()
    {
        Config.NewConfig<DbVotingTarget, VotingTargetResponse>()
            .Map(dest => dest.VotingId, src => src.VotingId.ToString())
            .Map(dest => dest.EducationUnitId, src => src.EducationUnitId.ToString());
        
        Config.NewConfig<AddVotingTargetRequest, DbVotingTarget>()
            .Map(dest => dest.VotingId, src => Guid.Parse(src.VotingId))
            .Map(dest => dest.EducationUnitId, src => Guid.Parse(src.EducationUnitId));
    }

    public static VotingTargetResponse MapToResponse(this DbVotingTarget source)
    {
        return source.Adapt<VotingTargetResponse>();
    }
    
    public static DbVotingTarget MapToEntity(this AddVotingTargetRequest request)
    {
        return request.Adapt<DbVotingTarget>();
    }
    
    public static IEnumerable<VotingTargetResponse> MapToResponseList(this IEnumerable<DbVotingTarget> source)
    {
        return source.Adapt<IEnumerable<VotingTargetResponse>>();
    }
}