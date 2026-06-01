using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Candidates;

public class CandidateServiceIntegrationTests(EduVoteApiFactory factory)
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
    public async Task CreateCandidate_Should_Add_Candidate_To_Voting()
    {
        var votingId = await SeedVotingAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/votings/{votingId}/candidates",
            new
            {
                votingId = votingId.ToString(),
                name = "Candidate A",
                description = "Description A"
            });

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<CandidateApiResponse>();
        Assert.NotNull(body);
        Assert.Equal("Candidate A", body.Name);
        Assert.Equal(votingId.ToString(), body.VotingId);

        var count = await CountCandidatesAsync(votingId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetCandidates_Should_Return_Created_Candidates()
    {
        var votingId = await SeedVotingAsync();

        await PostCandidateAsync(votingId, "First", "Desc 1");
        await PostCandidateAsync(votingId, "Second", "Desc 2");

        var response = await _client.GetAsync($"/api/votings/{votingId}/candidates");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetCandidatesApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Candidates.Count);
        Assert.Contains(body.Candidates, c => c.Name == "First");
        Assert.Contains(body.Candidates, c => c.Name == "Second");
    }

    [Fact]
    public async Task DeleteCandidate_Should_Remove_Candidate()
    {
        var votingId = await SeedVotingAsync();
        var createResponse = await PostCandidateAsync(votingId, "To delete", "Desc");
        await EnsureSuccessAsync(createResponse);

        var created = await createResponse.Content.ReadFromJsonAsync<CandidateApiResponse>();
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/votings/{votingId}/candidates/{created.Id}");

        await EnsureSuccessAsync(deleteResponse);
        Assert.Equal(0, await CountCandidatesAsync(votingId));
    }

    [Fact]
    public async Task CreateCandidate_Should_Return_NotFound_When_Voting_Does_Not_Exist()
    {
        var missingVotingId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/api/votings/{missingVotingId}/candidates",
            new
            {
                votingId = missingVotingId.ToString(),
                name = "Orphan",
                description = "Desc"
            });

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostCandidateAsync(Guid votingId, string name, string description) =>
        _client.PostAsJsonAsync(
            $"/api/votings/{votingId}/candidates",
            new
            {
                votingId = votingId.ToString(),
                name,
                description
            });

    private async Task<Guid> SeedVotingAsync()
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
                Title = "Voting with candidates",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = DbVotingStatus.Active,
                CreatedById = userId
            });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
    }

    private Task<int> CountCandidatesAsync(Guid votingId)
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Candidates.CountAsync(c => c.VotingId == votingId));
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private sealed record CandidateApiResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("votingId")] string VotingId,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("description")] string Description);

    private sealed record GetCandidatesApiResponse(
        [property: JsonPropertyName("candidates")] List<CandidateApiResponse> Candidates);
}
