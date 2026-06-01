using EduVote.Application.Analytics.Services;
using EduVote.Application.Auth.Services;
using EduVote.Application.Storage;
using EduVote.Application.Users.Services;
using EduVote.Application.Votings.Services;
using EduVote.Infrastructure.Analytics;
using EduVote.Infrastructure.Auth;
using EduVote.Infrastructure.Blockchain;
using EduVote.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace EduVote.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<BlockchainService>();
        services.AddScoped<IBlockchainResultWriter, BlockchainResultWriter>();
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IAnalyticsVotingReportPdfGenerator, VotingReportPdfGenerator>();
        services.AddSingleton<IAnalyticsOverviewReportPdfGenerator, OverviewReportPdfGenerator>();

        return services;
    }
}
