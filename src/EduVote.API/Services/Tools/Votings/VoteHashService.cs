using EduVote.DAL.Postgresql.Models;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.API.Services.Tools.Votings;

public class VoteHashService 
    : IVoteHashService
{
    public string GenerateVoteHash(
        Guid votingId,
        Guid userId,
        DbVotingType votingType,
        string salt,
        CastVoteRequest voteData)
    {
        var voteDataString = SerializeVoteData(votingType, voteData);
        
        var hashInput = $"{votingId}:{userId}:{votingType}:{voteDataString}:{salt}";
        
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    public string BuildHashInput(Vote vote, DbVotingType votingType)
    {
        var voteDataString = SerializeVoteDataFromModel(vote, votingType);
        return $"{vote.VotingId}:{vote.UserId}:{votingType}:{voteDataString}:{vote.VoteSalt}";
    }

    private static string SerializeVoteDataFromModel(Vote vote, DbVotingType votingType) =>
        votingType switch
        {
            DbVotingType.SingleChoice => vote.CandidateId!.Value.ToString(),
            DbVotingType.MultipleChoice => JsonSerializer.Serialize(
                (JsonSerializer.Deserialize<List<string>>(vote.SelectedCandidateIds!) ?? [])
                    .OrderBy(x => x)
            ),
            DbVotingType.Rating => JsonSerializer.Serialize(
                (JsonSerializer.Deserialize<Dictionary<string, int>>(vote.RatingAnswers!) ?? [])
                    .OrderBy(kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ),
            DbVotingType.OpenAnswer => vote.TextAnswer!,
            _ => throw new ArgumentException($"Unknown voting type: {votingType}")
        };

    private static string SerializeVoteData(DbVotingType votingType, CastVoteRequest voteData)
    {
        return votingType switch
        {
            DbVotingType.SingleChoice => voteData.SelectedCandidateId,
            DbVotingType.MultipleChoice => JsonSerializer.Serialize(
                voteData.SelectedCandidateIds.OrderBy(x => x)
            ),
            DbVotingType.Rating => JsonSerializer.Serialize(
                voteData.RatingAnswers
                    .OrderBy(kvp => kvp.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            ),
            DbVotingType.OpenAnswer => voteData.TextAnswer,
            _ => throw new ArgumentException($"Unknown voting type: {votingType}")
        };
    }
    
    /// <summary>
    /// ResultHash = SHA256(concat(lexicographically sorted VoteHashes)).
    /// Allows independent verification: anyone with the list of VoteHashes can recompute.
    /// </summary>
    public string GenerateResultHash(List<Vote> votes)
    {
        var combined = string.Concat(
            votes
                .Select(v => v.VoteHash)
                .OrderBy(h => h, StringComparer.Ordinal)
        );
        
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        
        return Convert.ToHexString(hashBytes).ToLower();
    }
}
