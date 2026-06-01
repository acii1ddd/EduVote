using MediatR;

namespace EduVote.Application.Votings.GetVoting;

public sealed record GetVotingQuery(Guid VotingId) : IRequest<VotingDetails>;
