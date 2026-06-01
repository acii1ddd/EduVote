using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories.Implementations;

public class VotingResultRepository(EduVoteDbContext dbContext) : IVotingResultRepository
{
    public async Task<VotingResult> CreateAsync(
        VotingResult votingResult,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(votingResult, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return votingResult;
    }

    public async Task<VotingResult?> GetByVotingIdAsync(
        Guid votingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.VotingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VotingId == votingId, cancellationToken);
    }

    public async Task<VotingResult?> GetByVotingIdForUpdateAsync(
        Guid votingId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.VotingResults
            .FirstOrDefaultAsync(x => x.VotingId == votingId, cancellationToken);
    }

    public async Task<VotingResult?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.VotingResults
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<VotingResult?> UpdateAsync(
        VotingResult votingResult,
        CancellationToken cancellationToken = default)
    {
        dbContext.VotingResults.Update(votingResult);
        await dbContext.SaveChangesAsync(cancellationToken);
        return votingResult;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
