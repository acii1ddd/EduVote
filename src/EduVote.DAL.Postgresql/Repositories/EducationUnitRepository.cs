using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
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

    public async Task<IEnumerable<Guid>> GetAllParentIdsAsync(
        IEnumerable<Guid> educationUnitIds,
        CancellationToken cancellationToken = default)
    {
        var unitIds = educationUnitIds.ToList();
        
        if (unitIds.Count == 0)
        {
            return [];
        }

        var result = await dbContext.EducationUnits
            .FromSqlInterpolated($"""

                                          WITH RECURSIVE parent_hierarchy AS (
                                              SELECT "Id", "ParentId"
                                              FROM "EducationUnits"
                                              WHERE "Id" IN (SELECT UNNEST({unitIds}))

                                              UNION ALL

                                              SELECT eu."Id", eu."ParentId"
                                              FROM "EducationUnits" eu
                                              JOIN parent_hierarchy ph 
                                                  ON eu."Id" = ph."ParentId"
                                          )
                                          SELECT DISTINCT "Id"
                                          FROM parent_hierarchy
                                      
                                  """)
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        return result;
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
