using EduVote.API.Services.Auth;
using EduVote.API.Services.Auth.PasswordHasher;
using EduVote.API.Services.CronJobs;
using EduVote.API.Services.Storage;
using EduVote.API.Services.Tools;
using EduVote.DAL.Postgresql.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;

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
            
            services.AddScoped<IFileStorageService, MinioFileStorageService>();

            // auth
            services.AddScoped<ITokenGenerator, JwtAccessTokenGenerator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            services.AddScoped<RegisterUserService>();
            services.AddScoped<LoginUserService>();
            
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
