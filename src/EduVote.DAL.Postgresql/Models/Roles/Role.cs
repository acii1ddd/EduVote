using EduVote.DAL.Postgresql.Models.Abstractions;

namespace EduVote.DAL.Postgresql.Models.Roles;

public class Role : IBaseEntity
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public ICollection<User> Users { get; set; } = [];
}