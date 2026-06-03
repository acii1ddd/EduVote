using System.Net.Http.Json;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class UpdateDeleteVotingIntegrationTests(EduVoteApiFactory factory)
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

    [Fact(DisplayName = "Обновление: меняет поля, статус не трогает")]
    public async Task UpdateVoting_Should_Update_Editable_Fields_And_Keep_Status()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Active);
        var startTime = DateTime.UtcNow.AddMinutes(10);
        var endTime = startTime.AddHours(3);

        var response = await PutVotingAsync(
            votingId,
            "Updated voting",
            "Updated description",
            "MultipleChoice",
            true,
            true,
            startTime,
            endTime);

        await EnsureSuccessAsync(response);

        var voting = await GetVotingAsync(votingId);
        Assert.Equal("Updated voting", voting.Title);
        Assert.Equal("Updated description", voting.Description);
        Assert.Equal(DbVotingType.MultipleChoice, voting.Type);
        Assert.True(voting.IsAnonymous);
        Assert.True(voting.AllowVoteChange);
        Assert.Equal(DbVotingStatus.Active, voting.Status);
    }

    [Fact(DisplayName = "Обновление: ошибка, если конец меньше чем через час после начала")]
    public async Task UpdateVoting_Should_Reject_When_EndTime_Is_Less_Than_One_Hour_After_StartTime()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Draft);
        var startTime = DateTime.UtcNow.AddMinutes(10);

        var response = await PutVotingAsync(
            votingId,
            "Invalid update",
            "Invalid update description",
            "SingleChoice",
            false,
            false,
            startTime,
            startTime.AddMinutes(30));

        Assert.False(response.IsSuccessStatusCode);

        var voting = await GetVotingAsync(votingId);
        Assert.Equal("Original voting", voting.Title);
        Assert.Equal(DbVotingStatus.Draft, voting.Status);
    }

    [Fact(DisplayName = "Удаление: голосование удаляется из БД")]
    public async Task DeleteVoting_Should_Remove_Voting()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Draft);

        var response = await _client.DeleteAsync($"/api/votings/{votingId}");

        await EnsureSuccessAsync(response);
        Assert.Equal(0, await CountVotingsAsync());
    }

    private Task<HttpResponseMessage> PutVotingAsync(
        Guid votingId,
        string title,
        string description,
        string type,
        bool isAnonymous,
        bool allowVoteChange,
        DateTime startTime,
        DateTime endTime)
    {
        return _client.PutAsJsonAsync($"/api/votings/{votingId}", new
        {
            id = votingId.ToString(),
            title,
            description,
            type,
            isAnonymous,
            allowVoteChange,
            startTime,
            endTime
        });
    }

    private async Task<Guid> SeedVotingAsync(DbVotingStatus status)
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
                Title = "Original voting",
                Description = "Original description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddMinutes(5),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = status,
                CreatedById = userId
            });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
    }

    private Task<Voting> GetVotingAsync(Guid votingId)
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Votings
                .AsNoTracking()
                .SingleAsync(v => v.Id == votingId));
    }

    private Task<int> CountVotingsAsync()
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Votings.CountAsync());
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }
}
