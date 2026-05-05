using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories;

public class EducationUnitRepository(EduVoteDbContext dbContext) 
    : IEducationUnitRepository
{
    public async Task<EducationUnit> CreateAsync(
        EducationUnit educationUnit,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(educationUnit, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return educationUnit;
    }

    public async Task<EducationUnit?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EducationUnits
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<EducationUnit>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EducationUnits
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EducationUnit>> GetByParentIdAsync(
        Guid parentId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.EducationUnits
            .AsNoTracking()
            .Where(x => x.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<EducationUnit?> UpdateAsync(
        EducationUnit educationUnit,
        CancellationToken cancellationToken = default)
    {
        dbContext.EducationUnits.Update(educationUnit);
        await dbContext.SaveChangesAsync(cancellationToken);

        return educationUnit;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        EducationUnit educationUnit,
        CancellationToken cancellationToken = default)
    {
        dbContext.EducationUnits.Remove(educationUnit);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
