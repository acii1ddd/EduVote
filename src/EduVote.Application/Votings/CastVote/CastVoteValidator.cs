using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Models;
using DbVoting = EduVote.DAL.Postgresql.Models.Voting;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Application.Votings.CastVote;

public static class CastVoteValidator
{
    private const int MinRatingValue = 1;
    private const int MaxRatingValue = 5;
    private const int MaxTextAnswerLength = 2000;

    public static void ValidateVote(
        CastVoteCommand command,
        DbVoting voting,
        IReadOnlyList<Candidate> candidates)
    {
        if (voting.Status != DbVotingStatus.Active)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.FailedPrecondition,
                $"Voting is not active. Current status: {voting.Status}");
        }

        var candidateIds = candidates
            .Select(c => c.Id)
            .ToList();

        switch (voting.Type)
        {
            case DbVotingType.SingleChoice:
                ValidateSingleChoice(command, candidateIds);
                break;
            case DbVotingType.MultipleChoice:
                ValidateMultipleChoice(command, candidateIds);
                break;
            case DbVotingType.Rating:
                ValidateRating(command, candidateIds);
                break;
            case DbVotingType.OpenAnswer:
                ValidateOpenAnswer(command);
                break;
            default:
                throw new ApplicationErrorException(
                    ApplicationErrorType.InvalidArgument,
                    $"Unknown voting type: {voting.Type}");
        }
    }

    private static void ValidateSingleChoice(CastVoteCommand command, IReadOnlyList<Guid> candidateIds)
    {
        if (string.IsNullOrWhiteSpace(command.SelectedCandidateId))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "For Single Choice voting, selected_candidate_id must be provided.");
        }

        if (!Guid.TryParse(command.SelectedCandidateId, out var candidateId))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "selected_candidate_id must be a valid GUID.");
        }

        if (!candidateIds.Contains(candidateId))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                $"Candidate with id {candidateId} is not available in this voting.");
        }
    }

    private static void ValidateMultipleChoice(CastVoteCommand command, IReadOnlyList<Guid> candidateIds)
    {
        if (command.SelectedCandidateIds.Count == 0)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "For Multiple Choice voting, at least one candidate must be selected.");
        }

        var parsedIds = new List<Guid>();
        foreach (var candidateIdStr in command.SelectedCandidateIds)
        {
            if (!Guid.TryParse(candidateIdStr, out var candidateId))
            {
                throw new ApplicationErrorException(
                    ApplicationErrorType.InvalidArgument,
                    $"Invalid candidate ID format: {candidateIdStr}");
            }

            parsedIds.Add(candidateId);
        }

        if (parsedIds.Distinct().Count() != parsedIds.Count)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "Duplicate candidate IDs are not allowed.");
        }

        var invalidCandidateId = parsedIds
            .FirstOrDefault(id => !candidateIds.Contains(id));

        if (invalidCandidateId != Guid.Empty)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                $"Candidate with id {invalidCandidateId} is not available in this voting.");
        }
    }

    private static void ValidateRating(CastVoteCommand command, IReadOnlyList<Guid> candidateIds)
    {
        if (command.RatingAnswers.Count == 0)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "For Rating voting, ratings for at least one candidate must be provided.");
        }

        var candidateIdSet = candidateIds.ToHashSet();

        foreach (var (candidateIdStr, rating) in command.RatingAnswers)
        {
            if (!Guid.TryParse(candidateIdStr, out var candidateId))
            {
                throw new ApplicationErrorException(
                    ApplicationErrorType.InvalidArgument,
                    $"Invalid candidate ID format: {candidateIdStr}");
            }

            if (!candidateIdSet.Contains(candidateId))
            {
                throw new ApplicationErrorException(
                    ApplicationErrorType.InvalidArgument,
                    $"Candidate with id {candidateId} is not available in this voting.");
            }

            if (rating is < MinRatingValue or > MaxRatingValue)
            {
                throw new ApplicationErrorException(
                    ApplicationErrorType.InvalidArgument,
                    $"Rating for candidate {candidateId} must be between {MinRatingValue} and {MaxRatingValue}, got {rating}.");
            }
        }
    }

    private static void ValidateOpenAnswer(CastVoteCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.TextAnswer))
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "For Open Answer voting, text_answer must be provided.");
        }

        if (command.TextAnswer.Length > MaxTextAnswerLength)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                $"Text answer must not exceed {MaxTextAnswerLength} characters.");
        }
    }
}
