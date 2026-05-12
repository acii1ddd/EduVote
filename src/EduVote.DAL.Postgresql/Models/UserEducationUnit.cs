namespace EduVote.DAL.Postgresql.Models;

public class UserEducationUnit
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid EducationUnitId { get; set; }
    public EducationUnit EducationUnit { get; set; } = null!;
}