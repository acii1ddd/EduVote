using EduVote.Application.Users;
using EduVote.DAL.Postgresql.Models;

namespace EduVote.API.Mappers;

public static class UserMapper
{
    private static readonly TypeAdapterConfig Config = new();

    static UserMapper()
    {
        // Mapping from DAL Candidate to gRPC CandidateResponse
        Config.NewConfig<User, UserResponse>()
            .Map(dest => dest.EducationUnitId,
                src => src.UserEducationUnits
                    .FirstOrDefault() != null
                    ? src.UserEducationUnits.First().EducationUnitId.ToString()
                    : "")
            .Map(dest => dest.EducationUnitName,
                src => src.UserEducationUnits
                    .FirstOrDefault() != null
                    ? src.UserEducationUnits.First().EducationUnit.Name
                    : "")
            .Map(dest => dest.Name, src => src.Name)
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

    public static UserResponse MapToResponse(this UserDetails source) =>
        new()
        {
            Id = source.Id.ToString(),
            Email = source.Email,
            Name = source.Name,
            Role = source.RoleName,
            EducationUnitId = source.EducationUnitId?.ToString() ?? string.Empty,
            EducationUnitName = source.EducationUnitName ?? string.Empty,
            CreatedAt = Timestamp.FromDateTime(source.CreatedAt.ToUniversalTime())
        };

    public static IEnumerable<UserResponse> MapToResponseList(this IEnumerable<UserDetails> source) =>
        source.Select(MapToResponse);
}
