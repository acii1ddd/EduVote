using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories;

public interface IVotingResultRepository
{
    Task<VotingResult> CreateAsync(VotingResult votingResult, CancellationToken cancellationToken = default);

    Task<VotingResult?> GetByVotingIdAsync(Guid votingId, CancellationToken cancellationToken = default);

    Task<VotingResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VotingResult?> UpdateAsync(VotingResult votingResult, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
