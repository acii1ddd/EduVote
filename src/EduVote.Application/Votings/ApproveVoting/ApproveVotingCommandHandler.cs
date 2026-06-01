using EduVote.Application.Votings.Services;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.ApproveVoting;

public sealed class ApproveVotingCommandHandler(VotingStatusService votingStatusService)
    : IRequestHandler<ApproveVotingCommand>
{
    public Task Handle(ApproveVotingCommand request, CancellationToken cancellationToken)
    {
        return votingStatusService.ChangeStatusAsync(
            request.VotingId,
            DbVotingStatus.Draft,
            [DbVotingStatus.PendingApproval],
            cancellationToken);
    }
}
