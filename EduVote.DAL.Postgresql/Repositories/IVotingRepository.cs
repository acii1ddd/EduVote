using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;

namespace EduVote.DAL.Postgresql.Services;

public interface IVotingRepository
{
    Task<Voting> CreateAsync(Voting voting, CancellationToken cancellationToken = default);
    
    Task<Voting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Voting?> UpdateAsync(Voting voting, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Voting?> UpdateStatusAsync(Guid id, VotingStatus status, CancellationToken cancellationToken = default);
}
