using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Analytics;

public class AnalyticsIntegrationTests(EduVoteApiFactory factory)
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
    public async Task GetOverview_Should_Return_Aggregated_Counters()
    {
        await SeedFinishedVotingWithResultAsync();

        var response = await _client.GetAsync("/api/analytics/overview");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<OverviewApiResponse>();
        Assert.NotNull(body);
        Assert.True(body.TotalVotings >= 1);
        Assert.True(body.FinishedVotings >= 1);
        Assert.NotEmpty(body.StatusCounts);
    }

    [Fact]
    public async Task GetVotings_Should_Return_Paginated_Items()
    {
        await SeedFinishedVotingWithResultAsync();

        var response = await _client.GetAsync("/api/analytics/votings?page=1&pageSize=10");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingsApiResponse>();
        Assert.NotNull(body);
        Assert.True(body.TotalCount >= 1);
        Assert.NotEmpty(body.Items);
        Assert.Equal(1, body.Page);
        Assert.Equal(10, body.PageSize);
    }

    [Fact]
    public async Task DownloadVotingReport_Should_Return_Pdf_For_Finished_Voting()
    {
        var votingId = await SeedFinishedVotingWithResultAsync();

        var response = await _client.GetAsync($"/api/analytics/votings/{votingId}/report.pdf");

        await EnsureSuccessAsync(response);

        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 100);
        Assert.Equal(0x25, bytes[0]); // %
        Assert.Equal(0x50, bytes[1]); // P
    }

    [Fact]
    public async Task DownloadVotingReport_Should_Return_FailedPrecondition_When_Not_Ready()
    {
        var votingId = await SeedDraftVotingAsync();

        var response = await _client.GetAsync($"/api/analytics/votings/{votingId}/report.pdf");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> SeedFinishedVotingWithResultAsync()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();
        var resultId = Guid.NewGuid();
        var candidateKey = candidateId.ToString();
        var resultData = $"{{\"{candidateKey}\":{{\"votes\":1}}}}";

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
                Title = "Analytics Finished Voting",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                Status = DbVotingStatus.Finished,
                CreatedById = userId
            });

            dbContext.Candidates.Add(new Candidate
            {
                Id = candidateId,
                VotingId = votingId,
                Name = "Candidate",
                Description = "Description"
            });

            dbContext.VotingResults.Add(new VotingResult
            {
                Id = resultId,
                VotingId = votingId,
                ResultData = resultData,
                ResultHash = "result-hash",
                CalculatedAt = DateTime.UtcNow.AddMinutes(-30),
                TotalVotes = 1
            });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
    }

    private async Task<Guid> SeedDraftVotingAsync()
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
                Title = "Draft voting",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddHours(1),
                EndTime = DateTime.UtcNow.AddHours(3),
                Status = DbVotingStatus.Draft,
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

    private sealed record OverviewApiResponse(
        [property: JsonPropertyName("totalVotings")] int TotalVotings,
        [property: JsonPropertyName("finishedVotings")] int FinishedVotings,
        [property: JsonPropertyName("statusCounts")] List<CountRow> StatusCounts);

    private sealed record CountRow(
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("count")] int Count);

    private sealed record VotingsApiResponse(
        [property: JsonPropertyName("items")] List<VotingRow> Items,
        [property: JsonPropertyName("totalCount")] int TotalCount,
        [property: JsonPropertyName("page")] int Page,
        [property: JsonPropertyName("pageSize")] int PageSize);

    private sealed record VotingRow(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("title")] string Title);
}
