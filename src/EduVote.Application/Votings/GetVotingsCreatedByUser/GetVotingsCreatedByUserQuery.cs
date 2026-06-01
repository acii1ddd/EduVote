using MediatR;

namespace EduVote.Application.Votings.GetVotingsCreatedByUser;

public sealed record GetVotingsCreatedByUserQuery(Guid UserId) : IRequest<IReadOnlyList<VotingDetails>>;
