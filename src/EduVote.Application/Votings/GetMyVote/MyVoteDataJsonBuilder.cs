using System.Text.Json;
using EduVote.DAL.Postgresql.Models;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Application.Votings.GetMyVote;

internal static class MyVoteDataJsonBuilder
{
    public static string Build(Vote vote, DbVotingType votingType, IReadOnlyList<Candidate> candidates)
    {
        object data = votingType switch
        {
            DbVotingType.SingleChoice => new
            {
                type = "SingleChoice",
                candidate = candidates
                    .Where(c => c.Id == vote.CandidateId)
                    .Select(c => new { id = c.Id.ToString(), name = c.Name })
                    .FirstOrDefault()
            },
            DbVotingType.MultipleChoice => new
            {
                type = "MultipleChoice",
                candidates = vote.GetSelectedCandidateIds()
                    .Select(id => candidates.FirstOrDefault(c => c.Id == id))
                    .Where(c => c is not null)
                    .Select(c => new { id = c!.Id.ToString(), name = c.Name })
                    .ToList()
            },
            DbVotingType.Rating => new
            {
                type = "Rating",
                ratings = vote.GetRatingAnswers()
                    .Select(kvp =>
                    {
                        var candidate = candidates.FirstOrDefault(c => c.Id == kvp.Key);
                        return new { id = kvp.Key.ToString(), name = candidate?.Name ?? "Unknown", rating = kvp.Value };
                    })
                    .ToList()
            },
            DbVotingType.OpenAnswer => new
            {
                type = "OpenAnswer",
                textAnswer = vote.TextAnswer ?? string.Empty
            },
            _ => new { type = "Unknown" }
        };

        return JsonSerializer.Serialize(data);
    }
}
