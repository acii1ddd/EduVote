using EduVote.Application.Common;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;

namespace EduVote.Application.Votings.DeleteVoting;

public sealed class DeleteVotingCommandHandler(IVotingRepository votingRepository)
    : IRequestHandler<DeleteVotingCommand>
{
    public async Task Handle(DeleteVotingCommand request, CancellationToken cancellationToken)
    {
        var voting = await votingRepository.GetByIdAsync(request.Id, cancellationToken);

        if (voting is null)
        {
            throw new ApplicationErrorException(
                ApplicationErrorType.NotFound,
                $"Voting with id {request.Id} was not found.");
        }

        await votingRepository.DeleteAsync(voting, cancellationToken);
    }
}
