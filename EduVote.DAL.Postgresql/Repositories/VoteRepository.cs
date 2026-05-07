using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories;

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

    public async Task<IEnumerable<Vote>> GetByVotingIdAsync(
        Guid votingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Votes
            .AsNoTracking()
            .Where(x => x.VotingId == votingId)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
