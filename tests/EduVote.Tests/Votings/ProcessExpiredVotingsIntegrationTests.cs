using EduVote.Application.Votings.ProcessExpiredVotings;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class ProcessExpiredVotingsIntegrationTests(EduVoteApiFactory factory)
    : IClassFixture<EduVoteApiFactory>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact(DisplayName = "Истечение срока: завершает только просроченные активные голосования")]
    public async Task ProcessExpiredVotings_Should_Finish_Only_Expired_Active_Votings()
    {
        var expiredVotingId = await SeedVotingAsync(
            DbVotingStatus.Active,
            endTime: DateTime.UtcNow.AddMinutes(-10));

        var activeVotingId = await SeedVotingAsync(
            DbVotingStatus.Active,
            endTime: DateTime.UtcNow.AddHours(2));

        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new ProcessExpiredVotingsCommand());

        Assert.Equal(1, result.ProcessedCount);
        Assert.Equal(DbVotingStatus.Finished, await GetVotingStatusAsync(expiredVotingId));
        Assert.Equal(DbVotingStatus.Active, await GetVotingStatusAsync(activeVotingId));
    }

    [Fact(DisplayName = "Истечение срока: завершает просроченное приостановленное голосование")]
    public async Task ProcessExpiredVotings_Should_Finish_Expired_Paused_Voting()
    {
        var votingId = await SeedVotingAsync(
            DbVotingStatus.Paused,
            endTime: DateTime.UtcNow.AddMinutes(-5));

        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new ProcessExpiredVotingsCommand());

        Assert.Equal(1, result.ProcessedCount);
        Assert.Equal(DbVotingStatus.Finished, await GetVotingStatusAsync(votingId));
    }

    private async Task<Guid> SeedVotingAsync(DbVotingStatus status, DateTime endTime)
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role { Id = roleId, Name = Roles.Administrator });
            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"admin-{userId:N}@example.com",
                Name = "Administrator",
                PasswordHash = "hash",
                RoleId = roleId
            });

            dbContext.Votings.Add(new Voting
            {
                Id = votingId,
                Title = "Expiration test voting",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = endTime,
                Status = status,
                CreatedById = userId
            });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
    }

    private Task<DbVotingStatus> GetVotingStatusAsync(Guid votingId)
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Votings
                .AsNoTracking()
                .Where(v => v.Id == votingId)
                .Select(v => v.Status)
                .SingleAsync());
    }
}
