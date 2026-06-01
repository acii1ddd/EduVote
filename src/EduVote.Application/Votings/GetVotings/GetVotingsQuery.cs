using MediatR;

namespace EduVote.Application.Votings.GetVotings;

public sealed record GetVotingsQuery : IRequest<IReadOnlyList<VotingDetails>>;
