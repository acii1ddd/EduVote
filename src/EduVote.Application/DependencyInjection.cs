using EduVote.Application.Votings.CastVote;
using EduVote.Application.Votings.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EduVote.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CastVoteCommand).Assembly));
        services.AddScoped<IVoteHashService, VoteHashService>();
        services.AddScoped<VotingStatusService>();
        services.AddScoped<VotingResultCalculatorService>();

        return services;
    }
}
