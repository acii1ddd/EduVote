using MediatR;

namespace EduVote.Application.Votings.CastVote;

public sealed record CastVoteCommand(
    Guid VotingId,
    Guid UserId,
    string SelectedCandidateId,
    IReadOnlyCollection<string> SelectedCandidateIds,
    IReadOnlyDictionary<string, int> RatingAnswers,
    string TextAnswer) : IRequest<CastVoteResult>;
