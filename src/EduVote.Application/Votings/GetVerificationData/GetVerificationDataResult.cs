namespace EduVote.Application.Votings.GetVerificationData;

public sealed record GetVerificationDataResult(
    Guid VotingId,
    string ResultHash,
    IReadOnlyList<string> VoteHashes,
    int TotalVotes);
