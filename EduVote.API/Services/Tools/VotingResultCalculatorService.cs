using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;

namespace EduVote.API.Services.Tools;

/// <summary>
/// Service for calculating voting results based on voting type
/// </summary>
public class VotingResultCalculatorService(IVoteRepository voteRepository)
{
    public async Task<VotingResult> CalculateVotingResultAsync(
        DbVoting votingModel,
        IEnumerable<Candidate> candidates,
        CancellationToken cancellationToken = default)
    {
        var votes = (await voteRepository
            .GetByVotingIdAsync(votingModel.Id, cancellationToken)
        ).ToList();
        
        var candidatesList = candidates.ToList();

        var resultData = votingModel.Type switch
        {
            DbVotingType.SingleChoice => CalculateSingleChoice(votes, candidatesList),
            DbVotingType.MultipleChoice => CalculateMultipleChoice(votes, candidatesList),
            DbVotingType.Rating => CalculateRating(votes, candidatesList),
            DbVotingType.OpenAnswer => CalculateOpenAnswer(votes),
            _ => new Dictionary<string, object>()
        };

        var resultJson = JsonSerializer.Serialize(resultData);
        var resultHash = GenerateResultHash(resultJson);

        return new VotingResult
        {
            Id = Guid.NewGuid(),
            VotingId = votingModel.Id,
            ResultData = resultJson,
            ResultHash = resultHash,
            CalculatedAt = DateTime.UtcNow,
            TotalVotes = votes.Count
        };
    }

    /// <summary>
    /// Single choice: count votes per candidate
    /// </summary>
    private Dictionary<string, object> CalculateSingleChoice(
        List<Vote> votes,
        List<Candidate> candidates)
    {
        var result = new Dictionary<string, object>();

        foreach (var candidate in candidates)
        {
            var count = votes.Count(v => v.CandidateId == candidate.Id);
            var key = candidate.Id.ToString();
            result[key] = new
            {
                candidateId = candidate.Id,
                candidateName = candidate.Name,
                voteCount = count,
                percentage = votes.Count > 0 ? Math.Round((count * 100.0) / votes.Count, 2) : 0
            };
        }

        return result;
    }

    /// <summary>
    /// Multiple choice: count how many times each candidate was selected
    /// </summary>
    private Dictionary<string, object> CalculateMultipleChoice(
        List<Vote> votes,
        List<Candidate> candidates)
    {
        var result = new Dictionary<string, object>();
        var voteCountPerCandidate = new Dictionary<Guid, int>();

        foreach (var candidate in candidates)
        {
            voteCountPerCandidate[candidate.Id] = 0;
        }

        foreach (var vote in votes)
        {
            var selectedIds = vote.GetSelectedCandidateIds();
            foreach (var candidateId in selectedIds)
            {
                if (voteCountPerCandidate.ContainsKey(candidateId))
                {
                    voteCountPerCandidate[candidateId]++;
                }
            }
        }

        foreach (var candidate in candidates)
        {
            var count = voteCountPerCandidate[candidate.Id];
            var key = candidate.Id.ToString();
            result[key] = new
            {
                candidateId = candidate.Id,
                candidateName = candidate.Name,
                selectionCount = count,
                percentage = votes.Count > 0 ? Math.Round((count * 100.0) / votes.Count, 2) : 0
            };
        }

        return result;
    }

    /// <summary>
    /// Rating: calculate average rating for each candidate
    /// </summary>
    private Dictionary<string, object> CalculateRating(
        List<Vote> votes,
        List<Candidate> candidates)
    {
        var result = new Dictionary<string, object>();
        var ratingSumPerCandidate = new Dictionary<Guid, (double sum, int count)>();

        foreach (var candidate in candidates)
        {
            ratingSumPerCandidate[candidate.Id] = (0, 0);
        }

        foreach (var vote in votes)
        {
            var ratings = vote.GetRatingAnswers();
            foreach (var (candidateId, rating) in ratings)
            {
                if (ratingSumPerCandidate.ContainsKey(candidateId))
                {
                    var current = ratingSumPerCandidate[candidateId];
                    ratingSumPerCandidate[candidateId] = (current.sum + rating, current.count + 1);
                }
            }
        }

        foreach (var candidate in candidates)
        {
            var (sum, count) = ratingSumPerCandidate[candidate.Id];
            var averageRating = count > 0 ? Math.Round(sum / count, 2) : 0;
            var key = candidate.Id.ToString();

            result[key] = new
            {
                candidateId = candidate.Id,
                candidateName = candidate.Name,
                averageRating = averageRating,
                totalRatings = count
            };
        }

        return result;
    }

    /// <summary>
    /// Open answer: return all text answers
    /// </summary>
    private Dictionary<string, object> CalculateOpenAnswer(List<Vote> votes)
    {
        var result = new Dictionary<string, object>();
        var answers = new List<string>();

        foreach (var vote in votes)
        {
            if (!string.IsNullOrEmpty(vote.TextAnswer))
            {
                answers.Add(vote.TextAnswer);
            }
        }

        result["totalAnswers"] = answers.Count;
        result["answers"] = answers;

        return result;
    }

    /// <summary>
    /// Generate SHA256 hash of result JSON for verification
    /// </summary>
    private static string GenerateResultHash(string resultJson)
    {
        var hashBytes = SHA256
            .HashData(Encoding.UTF8.GetBytes(resultJson));
        
        return Convert.ToHexString(hashBytes);
    }
}
