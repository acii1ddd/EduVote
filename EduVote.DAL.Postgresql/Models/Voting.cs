using EduVote.DAL.Postgresql.Models.Abstractions;
using EduVote.DAL.Postgresql.Models.Enums;

namespace EduVote.DAL.Postgresql.Models;

/// <summary>
/// Voting which include list of candidates
/// </summary>
public class Voting : IBaseEntity, ICreatedAt
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Voting title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Voting description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of voting (single, multiple, etc.)
    /// </summary>
    public VotingType Type  { get; set; }
    
    public bool IsAnonymous { get; set; }

    public bool AllowVoteChange { get; set; }
    
    /// <summary>
    /// Voting start time 
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Voting end time 
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Voting status
    /// </summary>
    // todo rename to Status instead of VotingStatus and make migration
    public VotingStatus Status { get; set; }
    
    /// <summary>
    /// Votes that already cast in this voting
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = [];
    
    /// <summary>
    /// All options for this vote
    /// </summary>
    public ICollection<Candidate> Candidates { get; set; } = [];
    
    /// <summary>
    /// All restrictions on users who can vote in this voting
    /// </summary>
    public ICollection<VotingTarget> VotingTargets { get; set; } = [];
    
    /// <summary>
    /// Voting result (calculated after voting ends)
    /// </summary>
    public VotingResult? VotingResult { get; set; }
    
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}