using EduVote.DAL.Postgresql.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EduVote.DAL.Postgresql;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDbInitializer()
        {
            services.AddScoped<DatabaseInitializer>();
        
            return services;
        }

        public IServiceCollection AddRepositories()
        {
            services.AddScoped<IVotingRepository, VotingRepository>();
        
            return services;
        }
    }
}