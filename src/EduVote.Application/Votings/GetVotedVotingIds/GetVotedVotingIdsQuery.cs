using MediatR;

namespace EduVote.Application.Votings.GetVotedVotingIds;

public sealed record GetVotedVotingIdsQuery(Guid UserId) : IRequest<IReadOnlyList<Guid>>;
