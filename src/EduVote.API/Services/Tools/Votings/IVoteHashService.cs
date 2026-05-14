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

    string GenerateResultHash(List<Vote> votes);
}