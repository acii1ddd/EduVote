namespace EduVote.DAL.Postgresql.Models;

public class Candidate
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Voting to which this candidate belongs
    /// </summary>
    public Guid VotingId { get; set; }

    public Voting Voting { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string? PhotoObjectName { get; set; }
    
    /// <summary>
    /// Votes that already cast for this candidate
    /// </summary>
    public ICollection<Vote> Votes { get; set; } = [];
}