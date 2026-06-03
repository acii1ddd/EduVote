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

    [Fact(DisplayName = "Проголосованные: возвращает id голосований после голоса пользователя")]
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

    [Fact(DisplayName = "Проголосованные: пустой список, если пользователь не голосовал")]
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

    [Fact(DisplayName = "Проголосованные: 401 без заголовков авторизации")]
    public async Task GetVotedVotingIds_Should_Return_Unauthenticated_Without_Auth_Headers()
    {
        var response = await _client.GetAsync("/api/votings/voted");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "Результаты: возвращает данные для завершённого голосования")]
    public async Task GetResults_Should_Return_Results_For_Finished_Voting()
    {
        var scenario = await SeedFinishedVotingWithResultAsync(includeBlockchain: true);
        AuthorizeAs(scenario.AdminUserId, Roles.Administrator);

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
        Assert.True(body.OpenAnswerTextsVisible);
    }

    [Fact(DisplayName = "Результаты open answer: студенту тексты ответов скрыты")]
    public async Task GetResults_OpenAnswer_Student_Should_Redact_Answer_Texts()
    {
        var scenario = await SeedFinishedOpenAnswerWithResultAsync();
        AuthorizeAs(scenario.StudentUserId, Roles.Student);

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/results");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingResultsApiResponse>();
        Assert.NotNull(body);
        Assert.False(body.OpenAnswerTextsVisible);
        Assert.NotNull(body.Results);
        Assert.True(body.Results.TryGetValue("openAnswer", out var openAnswer));
        var json = openAnswer.ToString();
        Assert.Contains("totalAnswers", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret answer", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Результаты open answer: организатор-преподаватель видит полные ответы")]
    public async Task GetResults_OpenAnswer_Organizer_Teacher_Should_Return_Full_Answers()
    {
        var scenario = await SeedFinishedOpenAnswerWithResultAsync();
        AuthorizeAs(scenario.OrganizerUserId, Roles.Teacher);

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/results");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingResultsApiResponse>();
        Assert.NotNull(body);
        Assert.True(body.OpenAnswerTextsVisible);
        Assert.NotNull(body.Results);
        Assert.True(body.Results.TryGetValue("openAnswer", out var openAnswer));
        var json = openAnswer.ToString();
        Assert.Contains("secret answer", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Результаты open answer: чужому преподавателю тексты скрыты")]
    public async Task GetResults_OpenAnswer_NonOrganizer_Teacher_Should_Redact_Answer_Texts()
    {
        var scenario = await SeedFinishedOpenAnswerWithResultAsync();
        AuthorizeAs(scenario.OtherTeacherUserId, Roles.Teacher);

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/results");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingResultsApiResponse>();
        Assert.NotNull(body);
        Assert.False(body.OpenAnswerTextsVisible);
        Assert.NotNull(body.Results);
        Assert.True(body.Results.TryGetValue("openAnswer", out var openAnswer));
        var json = openAnswer.ToString();
        Assert.DoesNotContain("secret answer", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Результаты open answer: администратор видит полные ответы даже не будучи организатором")]
    public async Task GetResults_OpenAnswer_Administrator_Should_Return_Full_Answers_Even_When_Not_Organizer()
    {
        var scenario = await SeedFinishedOpenAnswerWithResultAsync();
        AuthorizeAs(scenario.AdminUserId, Roles.Administrator);

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/results");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingResultsApiResponse>();
        Assert.NotNull(body);
        Assert.True(body.OpenAnswerTextsVisible);
        Assert.NotNull(body.Results);
        Assert.True(body.Results.TryGetValue("openAnswer", out var openAnswer));
        var json = openAnswer.ToString();
        Assert.Contains("secret answer", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Результаты open answer: 401 без авторизации")]
    public async Task GetResults_OpenAnswer_Should_Return_Unauthenticated_Without_Auth_Headers()
    {
        var scenario = await SeedFinishedOpenAnswerWithResultAsync();

        var response = await _client.GetAsync($"/api/votings/{scenario.VotingId}/results");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "Результаты: ошибка, если голосование ещё не завершено")]
    public async Task GetResults_Should_Return_FailedPrecondition_When_Voting_Is_Not_Finished()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Active);

        var response = await _client.GetAsync($"/api/votings/{votingId}/results");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Результаты: недоступны, пока итог ещё не посчитан")]
    public async Task GetResults_Should_Return_Unavailable_When_Result_Is_Not_Calculated_Yet()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Finished);

        var response = await _client.GetAsync($"/api/votings/{votingId}/results");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact(DisplayName = "Результаты: 404 для несуществующего голосования")]
    public async Task GetResults_Should_Return_NotFound_When_Voting_Does_Not_Exist()
    {
        var missingId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/votings/{missingId}/results");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Верификация: возвращает хеши голосов для завершённого голосования")]
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

    [Fact(DisplayName = "Верификация: ошибка, если голосование не завершено")]
    public async Task GetVerificationData_Should_Return_FailedPrecondition_When_Voting_Is_Not_Finished()
    {
        var votingId = await SeedVotingAsync(DbVotingStatus.Active);

        var response = await _client.GetAsync($"/api/votings/{votingId}/results/verification");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Верификация: недоступна, пока итог не посчитан")]
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

        return new FinishedVotingScenario(votingId, candidateId, userId);
    }

    private async Task<OpenAnswerScenario> SeedFinishedOpenAnswerWithResultAsync()
    {
        var teacherRoleId = Guid.NewGuid();
        var studentRoleId = Guid.NewGuid();
        var adminRoleId = Guid.NewGuid();
        var organizerUserId = Guid.NewGuid();
        var otherTeacherUserId = Guid.NewGuid();
        var studentUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var votingId = Guid.NewGuid();
        var resultId = Guid.NewGuid();
        const string resultData =
            """{"openAnswer":{"totalAnswers":1,"answers":["secret answer"]}}""";

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.AddRange(
                new Role { Id = teacherRoleId, Name = Roles.Teacher },
                new Role { Id = studentRoleId, Name = Roles.Student },
                new Role { Id = adminRoleId, Name = Roles.Administrator });

            dbContext.Users.AddRange(
                new User
                {
                    Id = organizerUserId,
                    Email = $"teacher-{organizerUserId:N}@example.com",
                    Name = "Organizer",
                    PasswordHash = "hash",
                    RoleId = teacherRoleId
                },
                new User
                {
                    Id = otherTeacherUserId,
                    Email = $"teacher-{otherTeacherUserId:N}@example.com",
                    Name = "Other teacher",
                    PasswordHash = "hash",
                    RoleId = teacherRoleId
                },
                new User
                {
                    Id = studentUserId,
                    Email = $"student-{studentUserId:N}@example.com",
                    Name = "Student",
                    PasswordHash = "hash",
                    RoleId = studentRoleId
                },
                new User
                {
                    Id = adminUserId,
                    Email = $"admin-{adminUserId:N}@example.com",
                    Name = "Administrator",
                    PasswordHash = "hash",
                    RoleId = adminRoleId
                });

            dbContext.Votings.Add(new Voting
            {
                Id = votingId,
                Title = "Open answer voting",
                Description = "Description",
                Type = DbVotingType.OpenAnswer,
                IsAnonymous = true,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                Status = DbVotingStatus.Finished,
                CreatedById = organizerUserId
            });

            dbContext.VotingResults.Add(new VotingResult
            {
                Id = resultId,
                VotingId = votingId,
                ResultData = resultData,
                ResultHash = "result-hash-open",
                CalculatedAt = DateTime.UtcNow.AddMinutes(-30),
                TotalVotes = 1
            });

            await dbContext.SaveChangesAsync();
        });

        return new OpenAnswerScenario(
            votingId,
            organizerUserId,
            otherTeacherUserId,
            studentUserId,
            adminUserId);
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

    private sealed record FinishedVotingScenario(Guid VotingId, Guid CandidateId, Guid AdminUserId);

    private sealed record OpenAnswerScenario(
        Guid VotingId,
        Guid OrganizerUserId,
        Guid OtherTeacherUserId,
        Guid StudentUserId,
        Guid AdminUserId);

    private sealed record GetVotedVotingIdsApiResponse(
        [property: JsonPropertyName("votingIds")] List<string> VotingIds);

    private sealed record VotingResultsApiResponse(
        [property: JsonPropertyName("votingId")] string VotingId,
        [property: JsonPropertyName("resultHash")] string ResultHash,
        [property: JsonPropertyName("totalVotes")] int TotalVotes,
        [property: JsonPropertyName("results")] Dictionary<string, object>? Results,
        [property: JsonPropertyName("txHash")] string? TxHash,
        [property: JsonPropertyName("etherscanUrl")] string? EtherscanUrl,
        [property: JsonPropertyName("openAnswerTextsVisible")] bool OpenAnswerTextsVisible);

    private sealed record VotingVerificationApiResponse(
        [property: JsonPropertyName("votingId")] string VotingId,
        [property: JsonPropertyName("resultHash")] string ResultHash,
        [property: JsonPropertyName("voteHashes")] List<string> VoteHashes,
        [property: JsonPropertyName("hashAlgorithm")] string HashAlgorithm,
        [property: JsonPropertyName("combineMethod")] string CombineMethod,
        [property: JsonPropertyName("totalVotes")] int TotalVotes);
}
