namespace EduVote.Application.Users;

public sealed record UserDetails(
    Guid Id,
    string Email,
    string Name,
    string RoleName,
    Guid? EducationUnitId,
    string? EducationUnitName,
    DateTime CreatedAt);
