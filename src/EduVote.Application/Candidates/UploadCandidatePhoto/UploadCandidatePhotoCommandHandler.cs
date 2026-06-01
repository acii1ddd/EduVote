using EduVote.Application.Common;
using EduVote.Application.Storage;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Candidates.UploadCandidatePhoto;

public sealed class UploadCandidatePhotoCommandHandler(
    ICandidateRepository candidateRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<UploadCandidatePhotoCommand, string>
{
    public async Task<string> Handle(
        UploadCandidatePhotoCommand request,
        CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository
            .GetByIdAsync(request.CandidateId, cancellationToken);

        if (candidate is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Candidate with id '{request.CandidateId}' was not found.");
        }

        if (request.PhotoStream.Length == 0)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.InvalidArgument,
                "Photo file is empty.");
        }

        var extension = Path.GetExtension(request.FileName);
        var objectName = $"{candidate.Name}{extension}";

        var photoUrl = await fileStorageService.UploadFileAsync(
            request.PhotoStream,
            request.ContentType,
            objectName,
            candidate.Id,
            cancellationToken);

        candidate.PhotoObjectName = objectName;
        await candidateRepository.SaveChangesAsync(cancellationToken);

        return photoUrl;
    }
}
