using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Candidates.DeleteCandidate;

public sealed class DeleteCandidateCommandHandler(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository)
    : IRequestHandler<DeleteCandidateCommand>
{
    public async Task Handle(DeleteCandidateCommand request, CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.VotingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id '{request.VotingId}' was not found.");
        }

        var candidate = await candidateRepository.GetByIdAsync(request.CandidateId, cancellationToken);

        if (candidate is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Candidate with id '{request.CandidateId}' was not found.");
        }

        await candidateRepository.DeleteAsync(candidate, cancellationToken);
    }
}
