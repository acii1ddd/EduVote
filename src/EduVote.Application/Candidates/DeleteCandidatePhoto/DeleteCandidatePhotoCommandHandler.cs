using EduVote.Application.Common;
using EduVote.Application.Storage;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Candidates.DeleteCandidatePhoto;

public sealed class DeleteCandidatePhotoCommandHandler(
    ICandidateRepository candidateRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<DeleteCandidatePhotoCommand>
{
    public async Task Handle(DeleteCandidatePhotoCommand request, CancellationToken cancellationToken)
    {
        var candidate = await candidateRepository
            .GetByIdAsync(request.CandidateId, cancellationToken);

        if (candidate is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Candidate with id '{request.CandidateId}' was not found.");
        }

        if (string.IsNullOrEmpty(candidate.PhotoObjectName))
            return;

        await fileStorageService.DeleteFileAsync(
            candidate.PhotoObjectName,
            candidate.Id,
            cancellationToken);

        candidate.PhotoObjectName = string.Empty;
        await candidateRepository.SaveChangesAsync(cancellationToken);
    }
}
