using MediatR;

namespace EduVote.Application.Votings.ProcessExpiredVotings;

public sealed record ProcessExpiredVotingsCommand 
    : IRequest<ProcessExpiredVotingsResult>;
