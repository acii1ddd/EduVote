using System.Net.Http.Json;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class ApproveVotingIntegrationTests(EduVoteApiFactory factory)
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

    [Fact(DisplayName = "Модерация: одобрение переводит из PendingApproval в Draft")]
    public async Task ApproveVoting_Should_Move_PendingApproval_To_Draft()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.PendingApproval);

        var response = await PostApproveVotingAsync(votingId);

        await EnsureSuccessAsync(response);
        Assert.Equal(DbVotingStatus.Draft, await GetVotingStatusAsync(votingId));
    }

    [Fact(DisplayName = "Модерация: отклонение при неверном статусе")]
    public async Task ApproveVoting_Should_Reject_When_Status_Is_Not_PendingApproval()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Draft);

        var response = await PostApproveVotingAsync(votingId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(DbVotingStatus.Draft, await GetVotingStatusAsync(votingId));
    }

    private Task<HttpResponseMessage> PostApproveVotingAsync(Guid votingId)
    {
        return _client.PostAsJsonAsync($"/api/votings/{votingId}/approve", new { });
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
                Name = Roles.Teacher
            });

            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"teacher-{userId:N}@example.com",
                Name = "Teacher",
                PasswordHash = "hash",
                RoleId = roleId
            });

            dbContext.Votings.Add(new Voting
            {
                Id = votingId,
                Title = "Approval voting",
                Description = "Voting for approve integration tests",
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
