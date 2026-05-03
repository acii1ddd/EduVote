namespace EduVote.DAL.Postgresql.Models.Abstractions;

public interface IBaseEntity
{
    public Guid Id { get; set; }
}