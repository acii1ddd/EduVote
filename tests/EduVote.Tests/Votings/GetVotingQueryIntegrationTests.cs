using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class GetVotingQueryIntegrationTests(EduVoteApiFactory factory)
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

    [Fact(DisplayName = "Голосование: получение по id")]
    public async Task GetVoting_Should_Return_Voting_By_Id()
    {
        var votingId = await SeedVotingAsync("Query voting", DbVotingStatus.Active);

        var response = await _client.GetAsync($"/api/votings/{votingId}");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(votingId.ToString(), body.Id);
        Assert.Equal("Query voting", body.Title);
        Assert.Equal("Test description", body.Description);
        Assert.Equal("SingleChoice", body.Type);
        Assert.False(body.IsAnonymous);
        Assert.True(body.AllowVoteChange);
        Assert.Equal("Active", body.Status);
        Assert.False(string.IsNullOrWhiteSpace(body.CreatedById));
    }

    [Fact(DisplayName = "Голосование: 404 для несуществующего id")]
    public async Task GetVoting_Should_Return_NotFound_When_Voting_Does_Not_Exist()
    {
        var missingId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/votings/{missingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Список голосований: возвращает все записи")]
    public async Task GetVotings_Should_Return_All_Votings()
    {
        await SeedVotingAsync("First voting", DbVotingStatus.Draft);
        await SeedVotingAsync("Second voting", DbVotingStatus.PendingApproval);

        var response = await _client.GetAsync("/api/votings");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotingsApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Votings.Count);
        Assert.Contains(body.Votings, v => v.Title == "First voting" && v.Status == "Draft");
        Assert.Contains(body.Votings, v => v.Title == "Second voting" && v.Status == "PendingApproval");
    }

    [Fact(DisplayName = "Список голосований: пустой список при отсутствии данных")]
    public async Task GetVotings_Should_Return_Empty_List_When_No_Votings()
    {
        var response = await _client.GetAsync("/api/votings");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotingsApiResponse>();
        Assert.NotNull(body);
        Assert.Empty(body.Votings);
    }

    private async Task<Guid> SeedVotingAsync(string title, DbVotingStatus status)
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
                Title = title,
                Description = "Test description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = true,
                StartTime = DateTime.UtcNow.AddMinutes(5),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = status,
                CreatedById = userId
            });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private sealed record VotingApiResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("isAnonymous")] bool IsAnonymous,
        [property: JsonPropertyName("allowVoteChange")] bool AllowVoteChange,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("createdById")] string CreatedById);

    private sealed record GetVotingsApiResponse(
        [property: JsonPropertyName("votings")] List<VotingApiResponse> Votings);
}
