using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories;

public interface IVotingTargetRepository
{
    Task<VotingTarget> CreateAsync(VotingTarget votingTarget, CancellationToken cancellationToken = default);

    Task<VotingTarget?> GetByVotingAndEducationUnitAsync(
        Guid votingId,
        Guid educationUnitId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VotingTarget>> GetByVotingIdAsync(Guid votingId, CancellationToken cancellationToken = default);

    Task<IEnumerable<VotingTarget>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task DeleteAsync(VotingTarget votingTarget, CancellationToken cancellationToken = default);
}
