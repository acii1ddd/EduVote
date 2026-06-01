using MediatR;

namespace EduVote.Application.Votings.DeleteVoting;

public sealed record DeleteVotingCommand(Guid Id) : IRequest;
