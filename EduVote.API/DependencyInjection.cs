using EduVote.API.Services.CronJobs;
using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Context;
using Microsoft.OpenApi.Models;

namespace EduVote.API;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiServices()
        {
            services.AddScoped<IVoteHashService, VoteHashService>();
            services.AddScoped<VotingResultCalculatorService>();
            services.AddScoped<VotingLifecycleService>();

            // singleton
            services.AddHostedService<VotingExpirationBgService>();
        
            return services;
        }

        public IServiceCollection AddGrpcServices()
        {
            services.AddGrpc().AddJsonTranscoding();
            services.AddGrpcReflection();
        
            return services;
        }

        public IServiceCollection AddSwaggerConf()
        {
            services.AddGrpcSwagger();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "gRPC transcoding", Version = "v1"
                });
            });
        
            return services;
        }
        
        public IServiceCollection AddDbContext(WebApplicationBuilder builder)
        {
            services.AddDbContext<EduVoteDbContext>(options =>
            {
                var connString = builder.Configuration.GetConnectionString("eduvote-db");
    
                options.UseNpgsql(connString);
            });
        
            return services;
        }
    }
}
