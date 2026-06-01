using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.VotingTargets.DeleteVotingTarget;

public sealed class DeleteVotingTargetCommandHandler(
    IVotingRepository votingRepository,
    IVotingTargetRepository votingTargetRepository)
    : IRequestHandler<DeleteVotingTargetCommand>
{
    public async Task Handle(
        DeleteVotingTargetCommand request,
        CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.VotingId, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id '{request.VotingId}' was not found.");
        }

        var target = await votingTargetRepository.GetByVotingAndEducationUnitAsync(
            request.VotingId,
            request.EducationUnitId,
            cancellationToken);

        if (target is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"VotingTarget with id '({request.VotingId}, {request.EducationUnitId})' was not found.");
        }

        await votingTargetRepository.DeleteAsync(target, cancellationToken);
    }
}
