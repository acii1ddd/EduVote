using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories.Implementations;

public class RoleRepository(EduVoteDbContext dbContext)
    : IRoleRepository
{
    public async Task<Role?> GetByNameAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(
                r => r.Name == roleName, cancellationToken
        );
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await dbContext.Roles
            .AsNoTracking().ToListAsync();
    }
}