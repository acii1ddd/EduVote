using MediatR;

namespace EduVote.Application.Candidates.CreateCandidate;

public sealed record CreateCandidateCommand(
    Guid VotingId,
    string Name,
    string Description) : IRequest<CandidateDetails>;
