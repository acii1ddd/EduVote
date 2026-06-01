using EduVote.Application.Storage;
using EduVote.DAL.Postgresql.Context;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace EduVote.Tests.Infrastructure;

public sealed class EduVoteApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17.6")
        .WithDatabase("eduvote_test")
        .WithUsername("user")
        .WithPassword("123")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:eduvote-db"] = _postgres.GetConnectionString(),
                ["ConnectionStrings:minio"] = "Endpoint=localhost:9000;AccessKey=user;SecretKey=admin123",
                ["AuthSettings:Issuer"] = TestJwtTokenFactory.Issuer,
                ["AuthSettings:Audience"] = TestJwtTokenFactory.Audience,
                ["AuthSettings:Secret"] = TestJwtTokenFactory.Secret,
                ["AuthSettings:LifeTime"] = "120",
                ["Blockchain:PrivateKey"] = "0000000000000000000000000000000000000000000000000000000000000001",
                ["Blockchain:InfuraUrl"] = "https://sepolia.infura.io/v3/test",
                ["Blockchain:MetaMaskPrivateKey"] = "0000000000000000000000000000000000000000000000000000000000000001"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.Configure<GrpcServiceOptions>(options =>
            {
                options.EnableDetailedErrors = true;
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFileStorageService>();
            services.AddScoped<IFileStorageService, FakeFileStorageService>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EduVoteDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<EduVoteDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EduVoteDbContext>();

        return await action(dbContext);
    }

    public async Task ExecuteDbContextAsync(Func<EduVoteDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EduVoteDbContext>();

        await action(dbContext);
    }
}
