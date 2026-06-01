using MediatR;

namespace EduVote.Application.Votings.GetVerificationData;

public sealed record GetVerificationDataQuery(Guid VotingId) : IRequest<GetVerificationDataResult>;
