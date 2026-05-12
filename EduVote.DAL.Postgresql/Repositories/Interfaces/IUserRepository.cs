using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdWithEducationUnitsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<User>> GetUsersWithRolesAndEducationUnitsAsync(CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Guid> AddAsync(User user, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default
    );
    
    Task DeleteAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
