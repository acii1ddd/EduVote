using MediatR;

namespace EduVote.Application.Votings.GetMyVote;

public sealed record GetMyVoteQuery(Guid VotingId, Guid UserId) : IRequest<GetMyVoteResult>;
