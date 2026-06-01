using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories.Implementations;

public class VoteRepository(EduVoteDbContext dbContext) : IVoteRepository
{
    public async Task<Vote> CreateAsync(Vote voteModel, 
        CancellationToken cancellationToken = default)
    {
        dbContext.Votes.Add(voteModel);
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return voteModel;
    }

    public async Task<Vote?> GetByIdAsync(Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Votes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Vote?> GetUserVoteAsync(Guid votingId, Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Votes
            .FirstOrDefaultAsync(x => x.VotingId == votingId && x.UserId == userId, 
                cancellationToken
            );
    }

    public async Task<IEnumerable<Guid>> GetVotedVotingIdsAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Votes
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.VotingId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vote>> GetByVotingIdAsync(
        Guid votingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Votes
            .AsNoTracking()
            .Where(x => x.VotingId == votingId)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteByVotingIdAsync(Guid votingId, CancellationToken cancellationToken = default) =>
        dbContext.Votes
            .Where(x => x.VotingId == votingId)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<Vote> votes,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Votes.AddRangeAsync(votes, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
