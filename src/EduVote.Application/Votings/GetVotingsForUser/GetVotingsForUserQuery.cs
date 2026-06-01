using MediatR;

namespace EduVote.Application.Votings.GetVotingsForUser;

public sealed record GetVotingsForUserQuery(Guid UserId) : IRequest<IReadOnlyList<VotingDetails>>;
