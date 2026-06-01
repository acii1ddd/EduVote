namespace EduVote.Application.Candidates;

public sealed record CandidateDetails(
    Guid Id,
    Guid VotingId,
    string Name,
    string Description,
    string PhotoUrl);
