using EduVote.DAL.Postgresql.Models.Abstractions;

namespace EduVote.DAL.Postgresql.Models;

/// <summary>
/// Dependent entity
/// Voting result calculated and stored for verification and blockchain integration
/// </summary>
public class VotingResult : IBaseEntity
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Voting to which this result belongs
    /// </summary>
    public Guid VotingId { get; set; }

    public Voting Voting { get; set; } = null!;
    
    /// <summary>
    /// Calculated result data as JSONB
    /// Format depends on VotingType:
    /// - Single: {candidateId: voteCount, ...}
    /// - Multiple: {candidateId: voteCount, ...}
    /// - Rating: {candidateId: averageRating, ...}
    /// - OpenAnswer: {answerId: textAnswer, ...}
    /// </summary>
    public string ResultData { get; set; } = string.Empty;
    
    /// <summary>
    /// Hash of the result for verification and blockchain integration
    /// </summary>
    public string ResultHash { get; set; } = string.Empty;
    
    /// <summary>
    /// When the result was calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; }
    
    /// <summary>
    /// Total number of votes that were counted
    /// </summary>
    public int TotalVotes { get; set; }
}
