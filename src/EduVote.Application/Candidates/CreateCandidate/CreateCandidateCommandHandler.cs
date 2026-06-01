using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Candidates.CreateCandidate;

public sealed class CreateCandidateCommandHandler(
    IVotingRepository votingRepository,
    ICandidateRepository candidateRepository)
    : IRequestHandler<CreateCandidateCommand, CandidateDetails>
{
    public async Task<CandidateDetails> Handle(
        CreateCandidateCommand request,
        CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.VotingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id '{request.VotingId}' was not found.");
        }

        var candidate = new Candidate
        {
            Id = Guid.NewGuid(),
            VotingId = request.VotingId,
            Name = request.Name,
            Description = request.Description
        };

        var created = await candidateRepository.CreateAsync(candidate, cancellationToken);

        return new CandidateDetails(
            created.Id,
            created.VotingId,
            created.Name,
            created.Description,
            string.Empty);
    }
}
