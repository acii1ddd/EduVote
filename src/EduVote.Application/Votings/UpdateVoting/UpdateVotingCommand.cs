using EduVote.DAL.Postgresql.Models.Enums;
using MediatR;

namespace EduVote.Application.Votings.UpdateVoting;

public sealed record UpdateVotingCommand(
    Guid Id,
    string Title,
    string Description,
    VotingType Type,
    bool IsAnonymous,
    bool AllowVoteChange,
    DateTime StartTime,
    DateTime EndTime) : IRequest<UpdateVotingResult>;
