using EduVote.DAL.Postgresql.Models.Abstractions;

namespace EduVote.DAL.Postgresql.Models;

/// <summary>
/// Who voted for what item (unique v.UserId, v.VotingId for one vote per voting)
/// </summary>
public class Vote : IBaseEntity, ICreatedAt
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Voting to which this vote is linked
    /// </summary>
    public Guid VotingId { get; set; }

    public Voting Voting { get; set; } = null!;
    
    /// <summary>
    /// User who votes
    /// </summary>
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
    
    /// <summary>
    /// The item of choice that was voted for
    /// </summary>
    // todo multiple, rating and open votings
    public Guid CandidateId { get; set; }

    public Candidate Candidate { get; set; } = null!;
    
    /// <summary>
    /// Hash of the vote
    /// </summary>
    public string VoteHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}