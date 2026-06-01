using MediatR;

namespace EduVote.Application.Candidates.DeleteCandidate;

public sealed record DeleteCandidateCommand(
    Guid VotingId,
    Guid CandidateId) : IRequest;
