using EduVote.DAL.Postgresql.Models.Enums;
using MediatR;

namespace EduVote.Application.Votings.CreateVoting;

public sealed record CreateVotingCommand(
    string Title,
    string Description,
    VotingType Type,
    bool IsAnonymous,
    bool AllowVoteChange,
    DateTime StartTime,
    DateTime EndTime,
    Guid? CreatedById,
    string? CreatorRole) : IRequest<CreateVotingResult>;
