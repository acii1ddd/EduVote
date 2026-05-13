using EduVote.DAL.Postgresql.Models.Abstractions;

namespace EduVote.DAL.Postgresql.Models;

/// <summary>
/// Blockchain record for voting result verification
/// Used for integration with Ethereum testnet (Sepolia) via Infura/Alchemy
/// </summary>
public class BlockchainRecord : IBaseEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Voting result this record is linked to
    /// </summary>
    public Guid VotingResultId { get; set; }

    public VotingResult VotingResult { get; set; } = null!;
    
    /// <summary>
    /// Voting ID (for reference)
    /// </summary>
    public Guid VotingId { get; set; }
    
    /// <summary>
    /// Transaction hash on blockchain
    /// </summary>
    public string TransactionHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash of all votes combined for integrity verification
    /// </summary>
    public string VotesHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Block number where the transaction was recorded
    /// </summary>
    public string BlockNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Blockchain network identifier ("sepolia", "mainnet", etc.)
    /// </summary>
    public string Network { get; set; } = "sepolia";
    
    /// <summary>
    /// When the record was created on blockchain
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Status of blockchain recording (pending, confirmed, failed)
    /// </summary>
    public string Status { get; set; } = "pending";
}
