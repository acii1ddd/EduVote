using MediatR;

namespace EduVote.Application.Votings.ApproveVoting;

public sealed record ApproveVotingCommand(Guid VotingId) : IRequest;
