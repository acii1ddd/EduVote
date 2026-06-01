using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EduVote.Application.Votings.CastVote;
using EduVote.DAL.Postgresql.Models;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Application.Votings.Services;

public sealed class VoteHashService : IVoteHashService
{
    public string GenerateVoteHash(
        Guid votingId,
        Guid userId,
        DbVotingType votingType,
        string salt,
        CastVoteCommand voteData)
    {
        var voteDataString = GetCanonicalVoteDataJson(votingType, voteData);
        var hashInput = $"{votingId}:{userId}:{votingType}:{voteDataString}:{salt}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));

        return Convert.ToHexString(hashBytes).ToLower();
    }

    public string BuildHashInput(Vote vote, DbVotingType votingType)
    {
        var voteDataString = GetCanonicalVoteDataJson(vote, votingType);

        return $"{vote.VotingId}:{vote.UserId}:{votingType}:{voteDataString}:{vote.VoteSalt}";
    }

    public string GetCanonicalVoteDataJson(DbVotingType votingType, CastVoteCommand voteData) =>
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
