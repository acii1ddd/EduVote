using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Repositories;
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
            
        var votingLyfecycleService = scope.ServiceProvider
            .GetRequiredService<VotingLifecycleService>();
            
        var now = DateTime.UtcNow;

        var votings = await votingRepository
            .GetAllAsync(stoppingToken);

        foreach (var voting in votings)
        {
            var isExpirable = voting.Status is DbVotingStatus.Active or DbVotingStatus.Paused or DbVotingStatus.PendingApproval;
            var isExpired   = now >= voting.EndTime;

            if (!isExpirable || !isExpired)
                continue;

            try
            {
                await votingLyfecycleService.FinalizeVotingAsync(voting.Id, stoppingToken);

                logger.LogInformation("Voting {VotingId} expired and was finalized", voting.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to finalize expired voting {VotingId}", voting.Id);
            }
        }
    }
}