using EduVote.Application.Votings.Services;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.StartVoting;

public sealed class StartVotingCommandHandler(VotingStatusService votingStatusService)
    : IRequestHandler<StartVotingCommand>
{
    public Task Handle(StartVotingCommand request, CancellationToken cancellationToken)
    {
        return votingStatusService.ChangeStatusAsync(
            request.VotingId,
            DbVotingStatus.Active,
            [DbVotingStatus.Draft, DbVotingStatus.Paused],
            cancellationToken);
    }
}
