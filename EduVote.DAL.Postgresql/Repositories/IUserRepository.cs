using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdWithEducationUnitsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<User>> GetUsersWithRolesAsync(CancellationToken cancellationToken = default);
}
