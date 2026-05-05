using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories;

public interface IEducationUnitRepository
{
    Task<EducationUnit> CreateAsync(EducationUnit educationUnit, CancellationToken cancellationToken = default);

    Task<EducationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<EducationUnit>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<EducationUnit>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<EducationUnit?> UpdateAsync(EducationUnit educationUnit, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task DeleteAsync(EducationUnit educationUnit, CancellationToken cancellationToken = default);
}
