using MediatR;

namespace EduVote.Application.Candidates.DeleteCandidatePhoto;

public sealed record DeleteCandidatePhotoCommand(Guid CandidateId) : IRequest;
