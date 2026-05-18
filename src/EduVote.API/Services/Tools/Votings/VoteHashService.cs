using EduVote.DAL.Postgresql.Models;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.API.Services.Tools.Votings;

public class VoteHashService(ILogger<VoteHashService> logger)
    : IVoteHashService
{
    public string GenerateVoteHash(
        Guid votingId,
        Guid userId,
        DbVotingType votingType,
        string salt,
        CastVoteRequest voteData)
    {
        var voteDataString = GetCanonicalVoteDataJson(votingType, voteData);

        logger.LogInformation("[GenerateVoteHash] Vote data JSON:\n{@VoteData}", voteDataString);

        var hashInput = $"{votingId}:{userId}:{votingType}:{voteDataString}:{salt}";
        
        logger.LogInformation("[GenerateVoteHash] HASH INPUT RAW: [{HashInput}]", hashInput);

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));

        var voteHash = Convert.ToHexString(hashBytes).ToLower();
        
        logger.LogInformation("[GenerateVoteHash] VOTE HASH: {VoteHash}", voteHash);
        
        return voteHash;
    }

    public string BuildHashInput(Vote vote, DbVotingType votingType)
    {
        var voteDataString = GetCanonicalVoteDataJson(vote, votingType);

        logger.LogInformation("[BuildHashInput] Vote data JSON:\n{@VoteData}", voteDataString);

        var hashInput = $"{vote.VotingId}:{vote.UserId}:{votingType}:{voteDataString}:{vote.VoteSalt}";
        
        logger.LogInformation("[BuildHashInput] HASH INPUT RAW: [{HashInput}]", hashInput);
        
        return hashInput;
    }

    public string GetCanonicalVoteDataJson(DbVotingType votingType, CastVoteRequest voteData) =>
        votingType switch
        {
            DbVotingType.SingleChoice => voteData.SelectedCandidateId,
            DbVotingType.MultipleChoice => SerializeSortedIds(voteData.SelectedCandidateIds),
            DbVotingType.Rating => SerializeSortedRatings(voteData.RatingAnswers),
            DbVotingType.OpenAnswer => voteData.TextAnswer,
            _ => throw new ArgumentException($"Unknown voting type: {votingType}")
        };

    private static string GetCanonicalVoteDataJson(Vote vote, DbVotingType votingType) =>
        votingType switch
        {
            DbVotingType.SingleChoice => vote.CandidateId!.Value.ToString(),
            DbVotingType.MultipleChoice => SerializeSortedIds(
                JsonSerializer.Deserialize<List<string>>(vote.SelectedCandidateIds!) ?? []),
            DbVotingType.Rating => SerializeSortedRatings(
                JsonSerializer.Deserialize<Dictionary<string, int>>(vote.RatingAnswers!) ?? []),
            DbVotingType.OpenAnswer => vote.TextAnswer!,
            _ => throw new ArgumentException($"Unknown voting type: {votingType}")
        };

    private static string SerializeSortedIds(IEnumerable<string> ids) =>
        JsonSerializer.Serialize(ids.OrderBy(x => x, StringComparer.Ordinal));

    private static string SerializeSortedRatings(IEnumerable<KeyValuePair<string, int>> ratings) =>
        JsonSerializer.Serialize(
            ratings
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

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
