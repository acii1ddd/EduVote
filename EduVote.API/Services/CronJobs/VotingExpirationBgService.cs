using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Repositories;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;

namespace EduVote.API.Services.CronJobs;

public class VotingExpirationBgService(
    IVotingRepository votingRepository, 
    VotingLifecycleService votingFinalizationService, 
    ILogger<VotingExpirationBgService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            // todo выбирать только active голосования
            var votings = await votingRepository
                .GetAllAsync(stoppingToken);

            foreach (var voting in votings)
            {
                var isActiveVoting = voting.Status != DbVotingStatus.Finished;
                var isExpiredVoting = now >= voting.EndTime; 
                
                if (isActiveVoting && isExpiredVoting)
                {
                    await votingFinalizationService.FinalizeVotingAsync(voting.Id, stoppingToken);
                    
                    logger.LogInformation("Voting with id {VotingId} was expired", voting.Id);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}