using MediatR;

namespace EduVote.Application.Votings.StartVoting;

public sealed record StartVotingCommand(Guid VotingId) : IRequest;
