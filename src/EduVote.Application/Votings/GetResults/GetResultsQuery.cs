using MediatR;

namespace EduVote.Application.Votings.GetResults;

public sealed record GetResultsQuery(
    Guid VotingId,
    Guid? CallerUserId,
    string? CallerRole) : IRequest<GetResultsResult>;
