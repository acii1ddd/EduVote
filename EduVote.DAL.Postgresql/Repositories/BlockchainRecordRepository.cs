using EduVote.DAL.Postgresql.Context;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduVote.DAL.Postgresql.Repositories;

public class BlockchainRecordRepository(EduVoteDbContext dbContext) : IBlockchainRecordRepository
{
    public async Task<BlockchainRecord> CreateAsync(
        BlockchainRecord record,
        CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(record, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<BlockchainRecord?> GetByVotingResultIdAsync(
        Guid votingResultId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.BlockchainRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VotingResultId == votingResultId, cancellationToken);
    }

    public async Task<BlockchainRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.BlockchainRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<BlockchainRecord?> UpdateAsync(
        BlockchainRecord record,
        CancellationToken cancellationToken = default)
    {
        dbContext.BlockchainRecords.Update(record);
        await dbContext.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
