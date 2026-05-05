using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories;

public class UserRepository(EduVoteDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdWithEducationUnitsAsync(Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Include(u => u.UserEducationUnits)
                .ThenInclude(ueu => ueu.EducationUnit)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }
}
