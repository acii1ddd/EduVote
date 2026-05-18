using EduVote.DAL.Postgresql.Models;

namespace EduVote.API.Services.Tools.Votings;
using DbVotingType = DAL.Postgresql.Models.Enums.VotingType;

public interface IVoteHashService
{
    string GenerateVoteHash(
        Guid votingId,
        Guid userId,
        DbVotingType votingType,
        string salt,
        CastVoteRequest voteData
    );

    /// <summary>
    /// Reconstructs the exact hash input string from a saved Vote model.
    /// Used to give the user the string they can verify independently.
    /// </summary>
    string BuildHashInput(Vote vote, DbVotingType votingType);

    /// <summary>
    /// Canonical JSON fragment used inside the hash input string.
    /// Store this exact value in the database so GetMyVote can reconstruct the same hash input.
    /// </summary>
    string GetCanonicalVoteDataJson(DbVotingType votingType, CastVoteRequest voteData);

    string GenerateResultHash(List<Vote> votes);
}