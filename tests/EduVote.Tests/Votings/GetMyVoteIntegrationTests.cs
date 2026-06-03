using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class GetMyVoteIntegrationTests(EduVoteApiFactory factory)
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

    [Fact(DisplayName = "Мой голос: детали после single choice")]
    public async Task GetMyVote_Should_Return_Vote_Details_After_SingleChoice_Vote()
    {
        var scenario = await SeedSingleChoiceScenarioAsync();
        AuthorizeAs(scenario.UserId, Roles.Student);

        var voteResponse = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.CandidateId);
        await EnsureSuccessAsync(voteResponse);

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/my-vote");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<MyVoteApiResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.VoteId));
        Assert.False(string.IsNullOrWhiteSpace(body.VoteHash));
        Assert.False(string.IsNullOrWhiteSpace(body.VoteSalt));
        Assert.False(string.IsNullOrWhiteSpace(body.HashInput));
        Assert.Contains(scenario.VotingId.ToString(), body.HashInput, StringComparison.Ordinal);

        Assert.Equal("SingleChoice", body.VoteData.GetProperty("type").GetString());
        Assert.Equal(
            scenario.CandidateId.ToString(),
            body.VoteData.GetProperty("candidate").GetProperty("id").GetString());
        Assert.Equal("First candidate", body.VoteData.GetProperty("candidate").GetProperty("name").GetString());
    }

    [Fact(DisplayName = "Мой голос: 404, если пользователь не голосовал")]
    public async Task GetMyVote_Should_Return_NotFound_When_User_Has_Not_Voted()
    {
        var scenario = await SeedSingleChoiceScenarioAsync();
        AuthorizeAs(scenario.UserId, Roles.Student);

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/my-vote");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Мой голос: 401 без авторизации")]
    public async Task GetMyVote_Should_Return_Unauthenticated_Without_Auth_Headers()
    {
        var scenario = await SeedSingleChoiceScenarioAsync();

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/my-vote");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "Мой голос: 404 для несуществующего голосования")]
    public async Task GetMyVote_Should_Return_NotFound_When_Voting_Does_Not_Exist()
    {
        var userId = await SeedUserAsync();
        AuthorizeAs(userId, Roles.Student);

        var missingVotingId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/votings/{missingVotingId}/my-vote");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private void AuthorizeAs(Guid userId, string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
    }

    private Task<HttpResponseMessage> PostSingleChoiceVoteAsync(Guid votingId, Guid candidateId)
    {
        return _client.PostAsJsonAsync($"/api/votings/{votingId}/vote", new
        {
            votingId = votingId.ToString(),
            selectedCandidateId = candidateId.ToString()
        });
    }

    private async Task<SingleChoiceScenario> SeedSingleChoiceScenarioAsync()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role { Id = roleId, Name = Roles.Student });
            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"student-{userId:N}@example.com",
                Name = "Student",
                PasswordHash = "hash",
                RoleId = roleId
            });

            dbContext.Votings.Add(new Voting
            {
                Id = votingId,
                Title = "My vote voting",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow.AddHours(1),
                Status = DbVotingStatus.Active,
                CreatedById = userId
            });

            dbContext.Candidates.Add(new Candidate
            {
                Id = candidateId,
                VotingId = votingId,
                Name = "First candidate",
                Description = "First candidate description"
            });

            await dbContext.SaveChangesAsync();
        });

        return new SingleChoiceScenario(userId, votingId, candidateId);
    }

    private async Task<Guid> SeedUserAsync()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role { Id = roleId, Name = Roles.Student });
            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"student-{userId:N}@example.com",
                Name = "Student",
                PasswordHash = "hash",
                RoleId = roleId
            });

            await dbContext.SaveChangesAsync();
        });

        return userId;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private sealed record SingleChoiceScenario(Guid UserId, Guid VotingId, Guid CandidateId);

    private sealed record MyVoteApiResponse(
        [property: JsonPropertyName("voteId")] string VoteId,
        [property: JsonPropertyName("voteHash")] string VoteHash,
        [property: JsonPropertyName("voteSalt")] string VoteSalt,
        [property: JsonPropertyName("hashInput")] string HashInput,
        [property: JsonPropertyName("voteData")] JsonElement VoteData);
}
