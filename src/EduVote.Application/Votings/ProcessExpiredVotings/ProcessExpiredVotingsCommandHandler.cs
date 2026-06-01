using EduVote.Application.Votings.FinishVoting;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.Application.Votings.ProcessExpiredVotings;

public sealed class ProcessExpiredVotingsCommandHandler(
    IVotingRepository votingRepository,
    ISender sender,
    ILogger<ProcessExpiredVotingsCommandHandler> logger)
    : IRequestHandler<ProcessExpiredVotingsCommand, ProcessExpiredVotingsResult>
{
    public async Task<ProcessExpiredVotingsResult> Handle(
        ProcessExpiredVotingsCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var votings = await votingRepository
            .GetAllAsync(cancellationToken);
        
        var processedCount = 0;

        foreach (var voting in votings)
        {
            var canFinish = voting.Status is DbVotingStatus.Active or DbVotingStatus.Paused;
            var isExpired = now >= voting.EndTime;

            if (!canFinish || !isExpired)
            {
                continue;
            }

            try
            {
                var result = await sender.Send(
                    new FinishVotingCommand(voting.Id),
                    cancellationToken);

                processedCount++;

                logger.LogInformation(
                    "Voting {VotingId} expired and was finalized. TxHash: {TxHash}",
                    voting.Id,
                    result.TxHash ?? "(none)");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to finalize expired voting {VotingId}", voting.Id);
            }
        }

        return new ProcessExpiredVotingsResult(processedCount);
    }
}
