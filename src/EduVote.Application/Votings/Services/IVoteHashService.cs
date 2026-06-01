using EduVote.Application.Votings.CastVote;
using EduVote.DAL.Postgresql.Models;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Application.Votings.Services;

public interface IVoteHashService
{
    string GenerateVoteHash(
        Guid votingId,
        Guid userId,
        DbVotingType votingType,
        string salt,
        CastVoteCommand voteData);

    string BuildHashInput(Vote vote, DbVotingType votingType);

    string GetCanonicalVoteDataJson(DbVotingType votingType, CastVoteCommand voteData);

    string GenerateResultHash(List<Vote> votes);
}
