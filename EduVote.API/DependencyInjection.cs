using EduVote.API.Services;
using EduVote.API.Services.Tools;

namespace EduVote.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<IVoteHashService, VoteHashService>();
        
        return services;
    }
}
