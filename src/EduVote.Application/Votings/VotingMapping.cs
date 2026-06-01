using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.Application.Votings;

internal static class VotingMapping
{
    public static VotingDetails ToDetails(this DbVoting voting) =>
        new(
            voting.Id,
            voting.Title,
            voting.Description,
            voting.Type,
            voting.IsAnonymous,
            voting.AllowVoteChange,
            voting.StartTime,
            voting.EndTime,
            voting.Status,
            voting.CreatedAt,
            voting.CreatedById);
}
