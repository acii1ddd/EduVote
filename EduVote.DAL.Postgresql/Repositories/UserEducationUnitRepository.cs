using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;

namespace EduVote.DAL.Postgresql.Repositories;

public class UserEducationUnitRepository(
    EduVoteDbContext dbContext)
    : IUserEducationUnitRepository
{
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