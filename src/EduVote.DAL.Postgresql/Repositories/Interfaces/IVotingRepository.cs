using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories.Interfaces;

public interface IVotingRepository
{
    Task<Voting> CreateAsync(Voting voting, CancellationToken cancellationToken = default);
    
    Task<Voting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Voting?> FindByTitleContainingAsync(
        string titleSubstring,
        CancellationToken cancellationToken = default);
    
    Task<Voting?> UpdateAsync(Voting voting, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task DeleteAsync(Voting voting, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Voting>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Voting>> GetVotingsForEducationUnitsAsync(
        IEnumerable<Guid> educationUnitIds,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Voting>> GetByCreatedByAsync(
        Guid createdById,
        CancellationToken cancellationToken = default);
}