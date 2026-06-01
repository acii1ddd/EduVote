using EduVote.Application;
using EduVote.API.Services.Analytics;
using EduVote.API.Services.Auth;
using EduVote.API.Services.Auth.PasswordHasher;
using EduVote.Application.Auth.Services;
using EduVote.Application.Storage;
using EduVote.Application.Users.Services;
using AppIPasswordHasher = EduVote.Application.Users.Services.IPasswordHasher;
using EduVote.API.Services.CronJobs;
using EduVote.API.Services.Storage;
using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using IBlockchainResultWriter = EduVote.Application.Votings.Services.IBlockchainResultWriter;

namespace EduVote.API;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiServices()
        {
            services.AddApplicationServices();
            services.AddScoped<BlockchainService>();
            services.AddScoped<IBlockchainResultWriter, BlockchainResultWriter>();

            // singleton
            services.AddHostedService<VotingExpirationBgService>();
            
            services.AddScoped<IFileStorageService, MinioFileStorageService>();

            // auth
            services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();
            services.AddScoped<AppIPasswordHasher, PasswordHasher>();

            services.AddSingleton<IVotingReportPdfGenerator, VotingReportPdfGenerator>();
            services.AddSingleton<IOverviewReportPdfGenerator, OverviewReportPdfGenerator>();
            
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
        
        public IServiceCollection AddJwtAuthentication(IConfiguration config)
        {
            var issuer = config["AuthSettings:Issuer"];
            var audience = config["AuthSettings:Audience"];
            var secret = config["AuthSettings:Secret"];
            
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
            // services.AddAuthorization(authOptions =>
            // {
            //     authOptions.AddPolicy
            //     (
            //         "Admin",
            //         policy =>
            //         {
            //             policy.RequireAuthenticatedUser();
            //             policy.RequireClaim(
            //                 ClaimTypes.Role, 
            //                 // allowed roles
            //                 nameof(UserRole.Admin)
            //             );
            //         }
            //     );
            //     
            //     authOptions.AddPolicy
            //     (
            //         "Default",
            //         policy =>
            //         {
            //             policy.RequireAuthenticatedUser();
            //             policy.RequireClaim(
            //                 ClaimTypes.Role, 
            //                 // allowed roles
            //                 nameof(UserRole.Default)
            //             );
            //         }
            //     );
            // });
            services.AddAuthorization();
            
            return services;
        }
    }
}
