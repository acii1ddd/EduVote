using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.API.Services.Tools;

public interface IVoteHashService
{
    string GenerateHash(
        Guid votingId,
        Guid userId,
        DbVotingType votingType,
        CastVoteRequest voteData
    );
}

public class VoteHashService : IVoteHashService
{
    public string GenerateHash(
        Guid votingId,
        Guid userId,
        DbVotingType votingType,
        CastVoteRequest voteData)
    {
        var voteDataString = SerializeVoteData(votingType, voteData);
        var timestamp = DateTime.UtcNow.Ticks; // 1 tick = 100 nano s from 01.01.0001 00:00:00

        var hashInput = $"{votingId}:{userId}:{votingType}:{voteDataString}:{timestamp}";
        
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(hashInput));
        
        return Convert.ToHexString(hashBytes);
    }

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
}
