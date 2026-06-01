using EduVote.Application.Common;
using EduVote.Application.Storage;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Candidates.GetCandidates;

public sealed class GetCandidatesQueryHandler(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository,
    IFileStorageService fileStorageService)
    : IRequestHandler<GetCandidatesQuery, IReadOnlyList<CandidateDetails>>
{
    public async Task<IReadOnlyList<CandidateDetails>> Handle(
        GetCandidatesQuery request,
        CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.VotingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id '{request.VotingId}' was not found.");
        }

        var candidates = await candidateRepository
            .GetByVotingIdAsync(request.VotingId, cancellationToken);

        var result = new List<CandidateDetails>();

        foreach (var candidate in candidates)
        {
            var photoUrl = await CandidatePhotoUrlResolver.ResolveAsync(
                candidate.Id,
                candidate.PhotoObjectName,
                fileStorageService);

            result.Add(new CandidateDetails(
                candidate.Id,
                candidate.VotingId,
                candidate.Name,
                candidate.Description,
                photoUrl));
        }

        return result;
    }
}
