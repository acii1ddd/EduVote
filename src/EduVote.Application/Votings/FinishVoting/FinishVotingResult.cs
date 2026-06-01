namespace EduVote.Application.Votings.FinishVoting;

public sealed record FinishVotingResult(
    Guid VotingId,
    string? TxHash,
    string? EtherscanUrl);
