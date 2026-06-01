using EduVote.Application;
using EduVote.API.Services.CronJobs;
using EduVote.DAL.Postgresql.Context;
using EduVote.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace EduVote.API;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiServices()
        {
            services.AddApplicationServices();
            services.AddInfrastructure();
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
                    Title = "gRPC transcoding",
                    Version = "v1"
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

        public IServiceCollection AddJwtAuthentication(IConfiguration config)
        {
            var issuer = config["AuthSettings:Issuer"];
            var audience = config["AuthSettings:Audience"];
            var secret = config["AuthSettings:Secret"];

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secret!)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }

        public IServiceCollection AddAuthorizationPolitics()
        {
            services.AddAuthorization();

            return services;
        }
    }
}
