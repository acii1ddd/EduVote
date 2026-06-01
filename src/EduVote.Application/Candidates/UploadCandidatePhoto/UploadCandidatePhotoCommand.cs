using MediatR;

namespace EduVote.Application.Candidates.UploadCandidatePhoto;

public sealed record UploadCandidatePhotoCommand(
    Guid CandidateId,
    Stream PhotoStream,
    string ContentType,
    string FileName) : IRequest<string>;
