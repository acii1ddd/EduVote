using EduVote.DAL.Postgresql.Models;

namespace EduVote.DAL.Postgresql.Repositories;

public interface IBlockchainRecordRepository
{
    Task<BlockchainRecord> CreateAsync(BlockchainRecord record, CancellationToken cancellationToken = default);

    Task<BlockchainRecord?> GetByVotingResultIdAsync(Guid votingResultId, CancellationToken cancellationToken = default);

    Task<BlockchainRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<BlockchainRecord?> UpdateAsync(BlockchainRecord record, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
