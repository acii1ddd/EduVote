using EduVote.DAL.Postgresql.Models.Abstractions;
using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.DAL.Postgresql.Models;

public class User : IBaseEntity, ICreatedAt
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public Guid RoleId { get; set; }

    public Role UserRole { get; set; } = null!;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Votes that already cast by this user
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = [];
    
    /// <summary>
    /// In which education units this user consists of
    /// </summary>
    public ICollection<UserEducationUnit> UserEducationUnits { get; set; } = [];
    
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}