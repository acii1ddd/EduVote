using EduVote.DAL.Postgresql.Models.Roles;

namespace EduVote.DAL.Postgresql.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetAllAsync();
}