using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;

namespace EduVote.DAL.Postgresql.Repositories;

public interface IVotingRepository
{
    Task<Voting> CreateAsync(Voting voting, CancellationToken cancellationToken = default);
    
    Task<Voting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Voting?> UpdateAsync(Voting voting, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task DeleteAsync(Voting voting, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Voting>> GetAllAsync(CancellationToken cancellationToken = default);
}
