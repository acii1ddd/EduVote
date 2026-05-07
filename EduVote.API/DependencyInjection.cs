using EduVote.API.Services.CronJobs;
using EduVote.API.Services.Tools;

namespace EduVote.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IVoteHashService, VoteHashService>();
        services.AddScoped<VotingResultCalculatorService>();
        services.AddScoped<VotingLifecycleService>();

        // singleton
        services.AddHostedService<VotingExpirationBgService>();
        
        return services;
    }
}
