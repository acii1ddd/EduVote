using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories.Implementations;

public class CandidateRepository(EduVoteDbContext dbContext) 
    : ICandidateRepository
{
    public async Task<Candidate> CreateAsync(Candidate candidate, 
        CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(candidate, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return candidate;
    }

    public async Task<Candidate?> GetByIdAsync(Guid id, 
        CancellationToken cancellationToken = default)
    {
       return await dbContext.Candidates
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Candidate>> GetByVotingIdAsync(Guid votingId, 
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Candidates
            .AsNoTracking()
            .Where(x => x.VotingId == votingId)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Candidate candidate, 
        CancellationToken cancellationToken = default)
    {
        dbContext.Candidates.Remove(candidate);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}