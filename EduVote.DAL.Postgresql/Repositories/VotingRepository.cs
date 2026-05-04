using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories;

public class VotingRepository(EduVoteDbContext dbContext) 
    : IVotingRepository
{
    public async Task<Voting> CreateAsync(Voting votingModel, CancellationToken cancellationToken = default)
    {
        dbContext.Votings.Add(votingModel);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return votingModel;
    }

    public async Task<Voting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Votings
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Voting?> UpdateAsync(Voting votingModel, CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);

        return votingModel;
    }
    
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Voting voting, CancellationToken cancellationToken = default)
    {
        dbContext.Votings.Remove(voting);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Voting>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Votings
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
