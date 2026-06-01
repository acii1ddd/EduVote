namespace EduVote.Application.EducationUnits;

public sealed record EducationUnitDetails(
    Guid Id,
    string Name,
    string Type,
    Guid? ParentId);
