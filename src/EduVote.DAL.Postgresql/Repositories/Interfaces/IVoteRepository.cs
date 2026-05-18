using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories.Interfaces;

public interface IVoteRepository
{
    Task<Vote> CreateAsync(Vote voteModel, CancellationToken cancellationToken = default);
    
    Task<Vote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Vote?> GetUserVoteAsync(Guid votingId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Votings that the user has voted in. Used to prevent multiple votes in the same voting.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<Guid>> GetVotedVotingIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all votes for a specific voting
    /// </summary>
    Task<IEnumerable<Vote>> GetByVotingIdAsync(Guid votingId, CancellationToken cancellationToken = default);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
