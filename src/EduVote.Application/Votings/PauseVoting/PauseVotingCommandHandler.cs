using EduVote.Application.Votings.Services;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.PauseVoting;

public sealed class PauseVotingCommandHandler(VotingStatusService votingStatusService)
    : IRequestHandler<PauseVotingCommand>
{
    public Task Handle(PauseVotingCommand request, CancellationToken cancellationToken)
    {
        return votingStatusService.ChangeStatusAsync(
            request.VotingId,
            DbVotingStatus.Paused,
            [DbVotingStatus.Active],
            cancellationToken);
    }
}
