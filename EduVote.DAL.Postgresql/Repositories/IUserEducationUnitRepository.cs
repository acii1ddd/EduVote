using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories;

public interface IUserEducationUnitRepository
{
    Task AddAsync(
        UserEducationUnit entity,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid userId,
        Guid educationUnitId,
        CancellationToken cancellationToken = default);
}