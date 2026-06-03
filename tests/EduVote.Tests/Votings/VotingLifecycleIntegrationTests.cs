using System.Net.Http.Json;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class VotingLifecycleIntegrationTests(EduVoteApiFactory factory)
    : IClassFixture<EduVoteApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact(DisplayName = "Жизненный цикл: запуск переводит Draft в Active")]
    public async Task StartVoting_Should_Move_Draft_Voting_To_Active()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Draft);

        var response = await PostVotingActionAsync(votingId, "start");

        await EnsureSuccessAsync(response);
        Assert.Equal(DbVotingStatus.Active, await GetVotingStatusAsync(votingId));
    }

    [Fact(DisplayName = "Жизненный цикл: запуск возобновляет Paused")]
    public async Task StartVoting_Should_Move_Paused_Voting_To_Active()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Paused);

        var response = await PostVotingActionAsync(votingId, "start");

        await EnsureSuccessAsync(response);
        Assert.Equal(DbVotingStatus.Active, await GetVotingStatusAsync(votingId));
    }

    [Fact(DisplayName = "Жизненный цикл: пауза переводит Active в Paused")]
    public async Task PauseVoting_Should_Move_Active_Voting_To_Paused()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Active);

        var response = await PostVotingActionAsync(votingId, "pause");

        await EnsureSuccessAsync(response);
        Assert.Equal(DbVotingStatus.Paused, await GetVotingStatusAsync(votingId));
    }

    [Fact(DisplayName = "Жизненный цикл: пауза недоступна для Draft")]
    public async Task PauseVoting_Should_Reject_Draft_Voting()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Draft);

        var response = await PostVotingActionAsync(votingId, "pause");

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(DbVotingStatus.Draft, await GetVotingStatusAsync(votingId));
    }

    [Fact(DisplayName = "Жизненный цикл: завершение переводит Active в Finished")]
    public async Task FinishVoting_Should_Move_Active_Voting_To_Finished()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Active, includeExistingResult: true);

        var response = await PostVotingActionAsync(votingId, "finish");

        await EnsureSuccessAsync(response);
        Assert.Equal(DbVotingStatus.Finished, await GetVotingStatusAsync(votingId));
    }

    private Task<HttpResponseMessage> PostVotingActionAsync(Guid votingId, string action)
    {
        return _client.PostAsJsonAsync($"/api/votings/{votingId}/{action}", new { });
    }

    private async Task<Guid> SeedVotingAsync(
        DbVotingStatus status,
        bool includeExistingResult = false)
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role
            {
                Id = roleId,
                Name = Roles.Administrator
            });

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
                Title = "Lifecycle voting",
                Description = "Voting for lifecycle integration tests",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow.AddHours(1),
                Status = status,
                CreatedById = userId
            });

            if (includeExistingResult)
            {
                dbContext.VotingResults.Add(new VotingResult
                {
                    Id = Guid.NewGuid(),
                    VotingId = votingId,
                    ResultData = "{}",
                    ResultHash = new string('a', 64),
                    CalculatedAt = DateTime.UtcNow,
                    TotalVotes = 0
                });
            }

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

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }
}
