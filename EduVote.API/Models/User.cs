namespace EduVote.API.Models;

public interface ICreatedAt
{
    public DateTime CreatedAt { get; }
};

public interface IBaseEntity
{
    public Guid Id { get; set; }
}

// [ProtoContract]
// public class User : IBaseEntity, ICreatedAt
// {
//     [ProtoMember(1)]
//     public Guid Id { get; set; }
//     
//     [ProtoMember(2)]
//     public string Email { get; set; } = string.Empty;
//     
//     // [ProtoMember(3)]
//     // public Guid RoleId { get; set; }
//     
//     [ProtoMember(3)]
//     public Role UserRole { get; set; }
//     
//     [ProtoMember(4)]
//     public DateTime CreatedAt { get; } = DateTime.UtcNow;
// }
//
// public enum Role
// {
//     Student = 0,
//     Teacher = 1,
//     Admin = 2
// }

// public class NewsItem
// {
//     
// }
//
// public class Notification
// {
//     
// }
//