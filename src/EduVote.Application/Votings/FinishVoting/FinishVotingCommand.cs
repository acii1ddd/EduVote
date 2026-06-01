using MediatR;

namespace EduVote.Application.Votings.FinishVoting;

public sealed record FinishVotingCommand(Guid VotingId) : IRequest<FinishVotingResult>;
