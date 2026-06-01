using EduVote.Application.Votings.FinishVoting;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
using MediatR;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.API.Services.CronJobs;

public class VotingExpirationBgService(
    IServiceScopeFactory scopeFactory,
    ILogger<VotingExpirationBgService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessExpiredVotingsAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessExpiredVotingsAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var votingRepository = scope.ServiceProvider
            .GetRequiredService<IVotingRepository>();

        var sender = scope.ServiceProvider
            .GetRequiredService<ISender>();

        var now = DateTime.UtcNow;

        var votings = await votingRepository
            .GetAllAsync(stoppingToken);

        foreach (var voting in votings)
        {
            var canFinish = voting.Status is DbVotingStatus.Active or DbVotingStatus.Paused;
            var isExpired = now >= voting.EndTime;

            if (!canFinish || !isExpired)
                continue;

            try
            {
                var result = await sender.Send(
                    new FinishVotingCommand(voting.Id),
                    stoppingToken);

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
    }
}
