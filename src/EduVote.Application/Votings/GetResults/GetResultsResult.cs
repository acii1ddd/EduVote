namespace EduVote.Application.Votings.GetResults;

public sealed record GetResultsResult(
    Guid VotingId,
    string ResultHash,
    DateTime CalculatedAt,
    int TotalVotes,
    string? ResultData,
    string? TxHash,
    string? EtherscanUrl,
    bool OpenAnswerTextsVisible);
