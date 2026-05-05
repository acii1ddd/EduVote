using EduVote.DAL.Postgresql.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EduVote.DAL.Postgresql;

public static class DependencyInjection
{
    public static IServiceCollection AddDbInitializer(this IServiceCollection services)
    {
        services.AddScoped<DatabaseInitializer>();
        
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVotingRepository, VotingRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IEducationUnitRepository, EducationUnitRepository>();
        services.AddScoped<IVotingTargetRepository, VotingTargetRepository>();
        
        return services;
    }
}