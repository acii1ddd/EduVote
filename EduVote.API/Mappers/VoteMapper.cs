using DbVote = EduVote.DAL.Postgresql.Models.Vote;

namespace EduVote.API.Mappers;

public static class VoteMapper
{
    private static readonly TypeAdapterConfig Config = new();

    static VoteMapper()
    {
        Config.NewConfig<DbVote, CastVoteResponse>()
            .Map(dest => dest.VoteId, src => src.Id.ToString())
            .Map(dest => dest.VoteHash, src => src.VoteHash)
            .Map(dest => dest.CreatedAt, src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime()));
    }

    public static CastVoteResponse MapToResponse(this DbVote source)
    {
        return source.Adapt<CastVoteResponse>(Config);
    }
}
