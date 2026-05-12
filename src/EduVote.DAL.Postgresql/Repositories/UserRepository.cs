using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories;

public class UserRepository(EduVoteDbContext dbContext) 
    : IUserRepository
{
    public async Task<User?> GetByIdWithEducationUnitsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(u => u.UserEducationUnits)
                .ThenInclude(ueu => ueu.EducationUnit)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersWithRolesAndEducationUnitsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .AsNoTracking()
                .Include(x => x.UserRole)
                .Include(u => u.UserEducationUnits)
                    .ThenInclude(ueu => ueu.EducationUnit)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<Guid> AddAsync(
        User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
     
        await dbContext.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
    
    public async Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken
            );

        if (user is null)
        {
            return;
        }

        dbContext.Users.Remove(user);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
