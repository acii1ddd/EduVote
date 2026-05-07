using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories;

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
}