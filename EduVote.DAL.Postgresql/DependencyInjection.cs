using EduVote.DAL.Postgresql.Repositories;
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
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<IEducationUnitRepository, EducationUnitRepository>();
            services.AddScoped<IVotingTargetRepository, VotingTargetRepository>();
            services.AddScoped<IVoteRepository, VoteRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
        
            return services;
        }
    }
}