using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories;

public interface ICandidateRepository
{
    Task<Candidate> CreateAsync(Candidate candidate, CancellationToken cancellationToken = default);
    
    Task<Candidate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Candidate>> GetByVotingIdAsync(Guid votingId, CancellationToken cancellationToken = default);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task DeleteAsync(Candidate candidate, CancellationToken cancellationToken = default);
}