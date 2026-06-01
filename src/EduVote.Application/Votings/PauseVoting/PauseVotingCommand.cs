using MediatR;

namespace EduVote.Application.Votings.PauseVoting;

public sealed record PauseVotingCommand(Guid VotingId) : IRequest;
