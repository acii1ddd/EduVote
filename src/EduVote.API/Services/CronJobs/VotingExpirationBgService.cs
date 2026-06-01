using EduVote.Application.Votings.ProcessExpiredVotings;
using MediatR;

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
            try
            {
                using var scope = scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                var result = await sender.Send(
                    new ProcessExpiredVotingsCommand(),
                    stoppingToken);

                if (result.ProcessedCount > 0)
                {
                    logger.LogInformation(
                        "Processed {Count} expired voting(s)",
                        result.ProcessedCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to process expired votings");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
