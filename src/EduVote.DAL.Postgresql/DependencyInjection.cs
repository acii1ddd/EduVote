using EduVote.DAL.Postgresql.Repositories;
using EduVote.DAL.Postgresql.Repositories.Implementations;
using EduVote.DAL.Postgresql.Repositories.Interfaces;
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
            services.AddScoped<IVotingResultRepository, VotingResultRepository>();
            services.AddScoped<IBlockchainRecordRepository, BlockchainRecordRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserEducationUnitRepository, UserEducationUnitRepository>();
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
            
            return services;
        }
    }
}