using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories.Implementations;

public class VotingTargetRepository(EduVoteDbContext dbContext) 
    : IVotingTargetRepository
{
    public async Task<VotingTarget> CreateAsync(
        VotingTarget votingTarget,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(votingTarget, cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return votingTarget;
    }
    
    public async Task<VotingTarget?> GetByVotingAndEducationUnitAsync(
        Guid votingId,
        Guid educationUnitId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.VotingTargets
            .FirstOrDefaultAsync(
                x => x.VotingId == votingId && x.EducationUnitId == educationUnitId,
                cancellationToken);
    }

    public async Task<IEnumerable<VotingTarget>> GetByVotingIdAsync(
        Guid votingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.VotingTargets
            .AsNoTracking()
            .Where(x => x.VotingId == votingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VotingTarget>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.VotingTargets
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        VotingTarget votingTarget,
        CancellationToken cancellationToken = default)
    {
        dbContext.VotingTargets.Remove(votingTarget);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
