using MediatR;

namespace EduVote.Application.Candidates.GetCandidates;

public sealed record GetCandidatesQuery(Guid VotingId) 
    : IRequest<IReadOnlyList<CandidateDetails>>;
