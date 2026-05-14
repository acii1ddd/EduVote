using System.Text.Json;
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
    /// The item of choice that was voted for (Single Choice)
    /// </summary>
    public Guid? CandidateId { get; set; }

    public Candidate? Candidate { get; set; }
    
    /// <summary>
    /// Selected candidate IDs for Multiple Choice voting (stored as JSON array of GUIDs)
    /// </summary>
    public string? SelectedCandidateIds { get; set; }

    /// <summary>
    /// Rating answers for Rating voting (stored as JSON object: {candidateId: rating})
    /// Format: {"candidateId1": 5, "candidateId2": 3, ...}
    /// </summary>
    public string? RatingAnswers { get; set; }

    /// <summary>
    /// Text answer for Open Answer voting
    /// </summary>
    public string? TextAnswer { get; set; }
    
    /// <summary>
    /// Random salt used when computing VoteHash.
    /// </summary>
    public string VoteSalt { get; set; } = string.Empty;

    /// <summary>
    /// Hash of the vote for verification and blockchain integration.
    /// SHA256(votingId:userId:votingType:voteData:salt)
    /// </summary>
    public string VoteHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Helper method to deserialize SelectedCandidateIds
    /// </summary>
    public List<Guid> GetSelectedCandidateIds()
    {
        if (string.IsNullOrEmpty(SelectedCandidateIds))
            return [];
        
        var ids = JsonSerializer
            .Deserialize<List<string>>(SelectedCandidateIds) ?? [];
        
        return ids.Select(Guid.Parse).ToList();
    }

    /// <summary>
    /// Helper method to deserialize RatingAnswers 
    /// Key : value (candidate guid : rating int)
    /// </summary>
    public Dictionary<Guid, int> GetRatingAnswers()
    {
        if (string.IsNullOrEmpty(RatingAnswers))
            return [];
        
        var jsonDict = JsonSerializer
            .Deserialize<Dictionary<string, int>>(RatingAnswers) ?? [];

        return jsonDict.ToDictionary(kvp => Guid.Parse(kvp.Key), kvp => kvp.Value);
    }
}