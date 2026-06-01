namespace EduVote.Application.Votings.GetMyVote;

public sealed record GetMyVoteResult(
    Guid VoteId,
    string VoteHash,
    string VoteSalt,
    string HashInput,
    string VoteDataJson);
