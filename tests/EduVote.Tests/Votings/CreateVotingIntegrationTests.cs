using System.Net.Http.Json;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class CreateVotingIntegrationTests(EduVoteApiFactory factory)
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

    [Fact]
    public async Task CreateVoting_Should_Create_Draft_Voting_For_Administrator()
    {
        var adminId = await SeedUserAsync(Roles.Administrator);
        AuthorizeAs(adminId, Roles.Administrator);

        var response = await PostCreateVotingAsync("Administrator voting");

        await EnsureSuccessAsync(response);

        var voting = await GetSingleVotingAsync();
        Assert.Equal("Administrator voting", voting.Title);
        Assert.Equal(DbVotingType.SingleChoice, voting.Type);
        Assert.Equal(DbVotingStatus.Draft, voting.Status);
        Assert.Equal(adminId, voting.CreatedById);
    }

    [Fact]
    public async Task CreateVoting_Should_Create_PendingApproval_Voting_For_Teacher()
    {
        var teacherId = await SeedUserAsync(Roles.Teacher);
        AuthorizeAs(teacherId, Roles.Teacher);

        var response = await PostCreateVotingAsync("Teacher voting");

        await EnsureSuccessAsync(response);

        var voting = await GetSingleVotingAsync();
        Assert.Equal("Teacher voting", voting.Title);
        Assert.Equal(DbVotingStatus.PendingApproval, voting.Status);
        Assert.Equal(teacherId, voting.CreatedById);
    }

    [Fact]
    public async Task CreateVoting_Should_Reject_When_EndTime_Is_Less_Than_One_Hour_After_StartTime()
    {
        var adminId = await SeedUserAsync(Roles.Administrator);
        AuthorizeAs(adminId, Roles.Administrator);

        var startTime = DateTime.UtcNow.AddMinutes(5);
        var response = await PostCreateVotingAsync(
            "Invalid voting",
            startTime,
            startTime.AddMinutes(30));

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(0, await CountVotingsAsync());
    }

    private void AuthorizeAs(Guid userId, string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
    }

    private Task<HttpResponseMessage> PostCreateVotingAsync(string title)
    {
        var startTime = DateTime.UtcNow.AddMinutes(5);
        return PostCreateVotingAsync(title, startTime, startTime.AddHours(2));
    }

    private Task<HttpResponseMessage> PostCreateVotingAsync(
        string title,
        DateTime startTime,
        DateTime endTime)
    {
        return _client.PostAsJsonAsync("/api/votings", new
        {
            title,
            description = "Voting created from integration tests",
            type = "SingleChoice",
            isAnonymous = false,
            allowVoteChange = false,
            startTime,
            endTime
        });
    }

    private async Task<Guid> SeedUserAsync(string roleName)
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role
            {
                Id = roleId,
                Name = roleName
            });

            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"{roleName.ToLowerInvariant()}-{userId:N}@example.com",
                Name = roleName,
                PasswordHash = "hash",
                RoleId = roleId
            });

            await dbContext.SaveChangesAsync();
        });

        return userId;
    }

    private Task<Voting> GetSingleVotingAsync()
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Votings.AsNoTracking().SingleAsync());
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
