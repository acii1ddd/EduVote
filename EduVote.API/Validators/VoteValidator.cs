using EduVote.DAL.Postgresql.Models;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;
using DbVotingStatus  = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.API.Validators;

public static class VoteValidator
{
    private const int MinRatingValue = 1;
    private const int MaxRatingValue = 5;
    private const int MaxTextAnswerLength = 2000;

    public static void ValidateVote(
        CastVoteRequest request,
        DbVoting voting,
        IReadOnlyList<Candidate> candidates)
    {
        if (voting.VotingStatus != DbVotingStatus.Active)
        {
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition,
                $"Voting is not active. Current status: {voting.VotingStatus}"));
        }

        var candidateIds = candidates
            .Select(c => c.Id)
            .ToList();

        switch (voting.Type)
        {
            case DAL.Postgresql.Models.Enums.VotingType.SingleChoice:
                ValidateSingleChoice(request, candidateIds);
                break;
            case DAL.Postgresql.Models.Enums.VotingType.MultipleChoice:
                ValidateMultipleChoice(request, candidateIds);
                break;
            case DAL.Postgresql.Models.Enums.VotingType.Rating:
                ValidateRating(request, candidateIds);
                break;
            case DAL.Postgresql.Models.Enums.VotingType.OpenAnswer:
                ValidateOpenAnswer(request);
                break;
            default:
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Unknown voting type: {voting.Type}"));
        }
    }

    private static void ValidateSingleChoice(CastVoteRequest request, IReadOnlyList<Guid> candidateIds)
    {
        if (string.IsNullOrWhiteSpace(request.SelectedCandidateId))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "For Single Choice voting, selected_candidate_id must be provided."));
        }

        // todo in extension method
        if (!Guid.TryParse(request.SelectedCandidateId, out var candidateId))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "selected_candidate_id must be a valid GUID."));
        }

        if (!candidateIds.Contains(candidateId))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Candidate with id {candidateId} is not available in this voting."));
        }
    }

    private static void ValidateMultipleChoice(CastVoteRequest request, IReadOnlyList<Guid> candidateIds)
    {
        if (request.SelectedCandidateIds.Count == 0)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "For Multiple Choice voting, at least one candidate must be selected."));
        }

        // todo in extension method
        var parsedIds = new List<Guid>();
        foreach (var candidateIdStr in request.SelectedCandidateIds)
        {
            if (!Guid.TryParse(candidateIdStr, out var candidateId))
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Invalid candidate ID format: {candidateIdStr}"));
            }
            parsedIds.Add(candidateId);
        }

        if (parsedIds.Distinct().Count() != parsedIds.Count)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Duplicate candidate IDs are not allowed."));
        }
        
        var invalidCandidateId = parsedIds
            .FirstOrDefault(id => !candidateIds.Contains(id));

        if (invalidCandidateId != Guid.Empty)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Candidate with id {invalidCandidateId} is not available in this voting."));
        }
    }

    private static void ValidateRating(CastVoteRequest request, IReadOnlyList<Guid> candidateIds)
    {
        if (request.RatingAnswers.Count == 0)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "For Rating voting, ratings for at least one candidate must be provided."));
        }

        var candidateIdSet = candidateIds.ToHashSet();

        foreach (var (candidateIdStr, rating) in request.RatingAnswers)
        {
            if (!Guid.TryParse(candidateIdStr, out var candidateId))
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Invalid candidate ID format: {candidateIdStr}"));
            }

            if (!candidateIdSet.Contains(candidateId))
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Candidate with id {candidateId} is not available in this voting."));
            }

            if (rating is < MinRatingValue or > MaxRatingValue)
            {
                throw new RpcException(new Status(
                    StatusCode.InvalidArgument,
                    $"Rating for candidate {candidateId} must be between {MinRatingValue} and {MaxRatingValue}, got {rating}."));
            }
        }
    }

    private static void ValidateOpenAnswer(CastVoteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TextAnswer))
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "For Open Answer voting, text_answer must be provided."));
        }

        if (request.TextAnswer.Length > MaxTextAnswerLength)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Text answer must not exceed {MaxTextAnswerLength} characters."));
        }
    }
}
