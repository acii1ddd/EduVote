using EduVote.DAL.Postgresql.Models.Abstractions;

namespace EduVote.DAL.Postgresql.Models;

public class User : IBaseEntity, ICreatedAt
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public Guid RoleId { get; set; }

    public Role UserRole { get; set; } = null!;
    
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}