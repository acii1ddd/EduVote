using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories.Implementations;

public class UserEducationUnitRepository(
    EduVoteDbContext dbContext)
    : IUserEducationUnitRepository
{
    public async Task ReplaceForUserAsync(
        Guid userId,
        Guid educationUnitId,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserEducationUnits
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            dbContext.UserEducationUnits.RemoveRange(existing);
        }

        await dbContext.UserEducationUnits.AddAsync(
            new UserEducationUnit
            {
                UserId = userId,
                EducationUnitId = educationUnitId
            },
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAsync(
        UserEducationUnit entity,
        CancellationToken cancellationToken = default)
    {
        dbContext.UserEducationUnits.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        Guid userId,
        Guid educationUnitId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.UserEducationUnits
            .FindAsync(
                [userId, educationUnitId],
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.UserEducationUnits.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
