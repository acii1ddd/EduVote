using EduVote.Application.Candidates;
using DbCandidate = EduVote.DAL.Postgresql.Models.Candidate;

namespace EduVote.API.Mappers;

public static class CandidateMapper
{
    private static readonly TypeAdapterConfig Config = new();

    static CandidateMapper()
    {
        // Mapping from DAL Candidate to gRPC CandidateResponse
        Config.NewConfig<DbCandidate, CandidateResponse>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.VotingId, src => src.VotingId.ToString())
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PhotoUrl, src => src.PhotoObjectName ?? string.Empty);

        // Mapping from gRPC AddCandidateRequest to DAL Candidate
        Config.NewConfig<AddCandidateRequest, DbCandidate>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.VotingId, src => Guid.Parse(src.VotingId))
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
    }

    public static CandidateResponse MapToResponse(this DbCandidate source)
    {
        return source.Adapt<CandidateResponse>(Config);
    }

    public static DbCandidate MapToEntity(this AddCandidateRequest request)
    {
        return request.Adapt<DbCandidate>(Config);
    }

    public static IEnumerable<CandidateResponse> MapToResponseList(this IEnumerable<DbCandidate> source)
    {
        return source.Adapt<IEnumerable<CandidateResponse>>(Config);
    }
    
    public static IEnumerable<DbCandidate> MapToEntityList(this IEnumerable<CandidateResponse>source)
    {
        return source.Adapt<IEnumerable<DbCandidate> >(Config);
    }

    public static CandidateResponse MapToResponse(this CandidateDetails source) =>
        new()
        {
            Id = source.Id.ToString(),
            VotingId = source.VotingId.ToString(),
            Name = source.Name,
            Description = source.Description,
            PhotoUrl = source.PhotoUrl
        };

    public static IEnumerable<CandidateResponse> MapToResponseList(
        this IEnumerable<CandidateDetails> source) =>
        source.Select(MapToResponse);
}
