using EduVote.DAL.Postgresql.Models.Enums;

namespace EduVote.Application.Votings.CreateVoting;

public sealed record CreateVotingResult(
    Guid Id,
    string Title,
    string Description,
    VotingType Type,
    bool IsAnonymous,
    bool AllowVoteChange,
    DateTime StartTime,
    DateTime EndTime,
    VotingStatus Status,
    DateTime CreatedAt,
    Guid CreatedById);
