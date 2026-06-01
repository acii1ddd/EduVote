using EduVote.DAL.Postgresql.Models.Enums;

namespace EduVote.Application.Votings.UpdateVoting;

public sealed record UpdateVotingResult(
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
