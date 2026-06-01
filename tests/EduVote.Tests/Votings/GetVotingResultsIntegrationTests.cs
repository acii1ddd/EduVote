using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class GetVotingResultsIntegrationTests(EduVoteApiFactory factory)
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
    public async Task GetVotedVotingIds_Should_Return_Voting_Ids_After_User_Voted()
    {
        var scenario = await SeedSingleChoiceScenarioAsync();
        AuthorizeAs(scenario.UserId, Roles.Student);

        var voteResponse = await _client.PostAsJsonAsync(
            $"/api/votings/{scenario.VotingId}/vote",
            new { selectedCandidateId = scenario.CandidateId.ToString() });
        await EnsureSuccessAsync(voteResponse);

        var response = await _client.GetAsync("/api/votings/voted");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotedVotingIdsApiResponse>();
        Assert.NotNull(body);
        Assert.Contains(scenario.VotingId.ToString(), body.VotingIds);
    }

    [Fact]
    public async Task GetVotedVotingIds_Should_Return_Empty_List_When_User_Has_Not_Voted()
    {
        var userId = await SeedUserAsync();
        AuthorizeAs(userId, Roles.Student);

        var response = await _client.GetAsync("/api/votings/voted");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotedVotingIdsApiResponse>();
        Assert.NotNull(body);
        Assert.Empty(body.VotingIds);
    }

    [Fact]
    public async Task GetVotedVotingIds_Should_Return_Unauthenticated_Without_Auth_Headers()
    {
        var response = await _client.GetAsync("/api/votings/voted");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetResults_Should_Return_Results_For_Finished_Voting()
    {
        var scenario = await SeedFinishedVotingWithResultAsync(includeBlockchain: true);

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/results");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingResultsApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(scenario.VotingId.ToString(), body.VotingId);
        Assert.Equal("result-hash-abc", body.ResultHash);
        Assert.Equal(2, body.TotalVotes);
        Assert.NotNull(body.Results);
        Assert.True(body.Results.ContainsKey(scenario.CandidateId.ToString()));
        Assert.Equal("blockchain-tx-hash", body.TxHash);
        Assert.Contains("sepolia.etherscan.io", body.EtherscanUrl ?? string.Empty);
    }

    [Fact]
    public async Task GetResults_Should_Return_FailedPrecondition_When_Voting_Is_Not_Finished()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Active);

        var response = await _client.GetAsync($"/api/votings/{votingId}/results");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetResults_Should_Return_Unavailable_When_Result_Is_Not_Calculated_Yet()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Finished);

        var response = await _client.GetAsync($"/api/votings/{votingId}/results");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task GetResults_Should_Return_NotFound_When_Voting_Does_Not_Exist()
    {
        var missingId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/votings/{missingId}/results");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetVerificationData_Should_Return_Vote_Hashes_For_Finished_Voting()
    {
        var scenario = await SeedFinishedVotingWithResultAsync(includeVotes: true);

        var response = await _client.GetAsync(
            $"/api/votings/{scenario.VotingId}/results/verification");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingVerificationApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(scenario.VotingId.ToString(), body.VotingId);
        Assert.Equal("result-hash-abc", body.ResultHash);
        Assert.Equal("SHA-256", body.HashAlgorithm);
        Assert.Equal("sort_ordinal_concat_no_separator", body.CombineMethod);
        Assert.Equal(1, body.TotalVotes);
        Assert.Single(body.VoteHashes);
        Assert.Equal("vote-hash-1", body.VoteHashes[0]);
    }

    [Fact]
    public async Task GetVerificationData_Should_Return_FailedPrecondition_When_Voting_Is_Not_Finished()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Active);

        var response = await _client.GetAsync($"/api/votings/{votingId}/results/verification");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetVerificationData_Should_Return_Unavailable_When_Result_Is_Not_Calculated_Yet()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Finished);

        var response = await _client.GetAsync($"/api/votings/{votingId}/results/verification");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private void AuthorizeAs(Guid userId, string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
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
                Title = "Active voting",
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
                Name = "Candidate",
                Description = "Description"
            });

            await dbContext.SaveChangesAsync();
        });

        return new SingleChoiceScenario(userId, votingId, candidateId);
    }

    private async Task<FinishedVotingScenario> SeedFinishedVotingWithResultAsync(
        bool includeBlockchain = false,
        bool includeVotes = false)
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();
        var resultId = Guid.NewGuid();
        var candidateKey = candidateId.ToString();
        var resultData = $"{{\"{candidateKey}\":{{\"votes\":2}}}}";

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
                Title = "Finished voting",
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
                ResultHash = "result-hash-abc",
                CalculatedAt = DateTime.UtcNow.AddMinutes(-30),
                TotalVotes = 2
            });

            if (includeVotes)
            {
                dbContext.Votes.Add(new Vote
                {
                    Id = Guid.NewGuid(),
                    VotingId = votingId,
                    UserId = userId,
                    CandidateId = candidateId,
                    VoteHash = "vote-hash-1",
                    VoteSalt = "salt-1"
                });
            }

            if (includeBlockchain)
            {
                dbContext.BlockchainRecords.Add(new BlockchainRecord
                {
                    Id = Guid.NewGuid(),
                    VotingResultId = resultId,
                    VotingId = votingId,
                    TransactionHash = "blockchain-tx-hash",
                    VotesHash = "result-hash-abc",
                    BlockNumber = "12345",
                    Network = "sepolia",
                    Status = "confirmed",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-20)
                });
            }

            await dbContext.SaveChangesAsync();
        });

        return new FinishedVotingScenario(votingId, candidateId);
    }

    private async Task<Guid> SeedVotingAsync(DbVotingStatus status)
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
                Title = "Voting",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                Status = status,
                CreatedById = userId
            });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
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

    private sealed record FinishedVotingScenario(Guid VotingId, Guid CandidateId);

    private sealed record GetVotedVotingIdsApiResponse(
        [property: JsonPropertyName("votingIds")] List<string> VotingIds);

    private sealed record VotingResultsApiResponse(
        [property: JsonPropertyName("votingId")] string VotingId,
        [property: JsonPropertyName("resultHash")] string ResultHash,
        [property: JsonPropertyName("totalVotes")] int TotalVotes,
        [property: JsonPropertyName("results")] Dictionary<string, object>? Results,
        [property: JsonPropertyName("txHash")] string? TxHash,
        [property: JsonPropertyName("etherscanUrl")] string? EtherscanUrl);

    private sealed record VotingVerificationApiResponse(
        [property: JsonPropertyName("votingId")] string VotingId,
        [property: JsonPropertyName("resultHash")] string ResultHash,
        [property: JsonPropertyName("voteHashes")] List<string> VoteHashes,
        [property: JsonPropertyName("hashAlgorithm")] string HashAlgorithm,
        [property: JsonPropertyName("combineMethod")] string CombineMethod,
        [property: JsonPropertyName("totalVotes")] int TotalVotes);
}
