using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.VotingTargets.AddVotingTarget;

public sealed class AddVotingTargetCommandHandler(
    IVotingRepository votingRepository,
    IEducationUnitRepository educationUnitRepository,
    IVotingTargetRepository votingTargetRepository)
    : IRequestHandler<AddVotingTargetCommand, VotingTargetDetails>
{
    public async Task<VotingTargetDetails> Handle(
        AddVotingTargetCommand request,
        CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.VotingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id '{request.VotingId}' was not found.");
        }

        var educationUnit = await educationUnitRepository.GetByIdAsync(
            request.EducationUnitId,
            cancellationToken);

        if (educationUnit is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Education unit with id '{request.EducationUnitId}' was not found.");
        }

        var existing = await votingTargetRepository.GetByVotingAndEducationUnitAsync(
            request.VotingId,
            request.EducationUnitId,
            cancellationToken);

        if (existing is not null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.AlreadyExists,
                $"Target for voting '{request.VotingId}' and education unit '{request.EducationUnitId}' already exists.");
        }

        var created = await votingTargetRepository.CreateAsync(
            new VotingTarget
            {
                VotingId = request.VotingId,
                EducationUnitId = request.EducationUnitId
            },
            cancellationToken);

        return created.ToDetails();
    }
}
