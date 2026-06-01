namespace EduVote.Application.Votings.CastVote;

public sealed record CastVoteResult(
    Guid VoteId,
    string VoteHash,
    DateTime CreatedAt,
    string VoteSalt);
