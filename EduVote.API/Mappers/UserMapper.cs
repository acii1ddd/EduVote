using EduVote.DAL.Postgresql.Models;

namespace EduVote.API.Mappers;

public static class UserMapper
{
    private static readonly TypeAdapterConfig Config = new();

    static UserMapper()
    {
        // Mapping from DAL Candidate to gRPC CandidateResponse
        Config.NewConfig<User, UserResponse>()
            .Map(dest => dest.Id, src => src.Id.ToString())
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Role, src => src.UserRole.Name)
            .Map(dest => dest.CreatedAt, src => Timestamp.FromDateTime(src.CreatedAt.ToUniversalTime()));
    }

    public static UserResponse MapToResponse(this User source)
    {
        return source.Adapt<UserResponse>(Config);
    }
    
    public static IEnumerable<UserResponse> MapToResponseList(this IEnumerable<User> source)
    {
        return source.Adapt<IEnumerable<UserResponse>>(Config);
    }
}
