using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.VotingTargets;

public class VotingTargetServiceIntegrationTests(EduVoteApiFactory factory)
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
    public async Task GetTargets_Should_Return_Existing_Targets()
    {
        var unitId = Guid.NewGuid();
        var votingId = await SeedVotingWithTargetAsync(unitId);

        var response = await _client.GetAsync($"/api/votings/{votingId}/targets");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotingTargetsApiResponse>();
        Assert.NotNull(body);
        Assert.Single(body.Targets);
        Assert.Equal(votingId.ToString(), body.Targets[0].VotingId);
        Assert.Equal(unitId.ToString(), body.Targets[0].EducationUnitId);
    }

    [Fact]
    public async Task AddTarget_Should_Create_Target_In_Database()
    {
        var (votingId, unitId) = await SeedVotingAndEducationUnitAsync();

        var response = await PostTargetAsync(votingId, unitId);

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<VotingTargetApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(votingId.ToString(), body.VotingId);
        Assert.Equal(unitId.ToString(), body.EducationUnitId);

        var exists = await TargetExistsAsync(votingId, unitId);
        Assert.True(exists);
    }

    [Fact]
    public async Task AddTarget_Should_Return_Conflict_When_Target_Already_Exists()
    {
        var unitId = Guid.NewGuid();
        var votingId = await SeedVotingWithTargetAsync(unitId);

        var response = await PostTargetAsync(votingId, unitId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTarget_Should_Remove_Target()
    {
        var unitId = Guid.NewGuid();
        var votingId = await SeedVotingWithTargetAsync(unitId);

        var response = await _client.DeleteAsync(
            $"/api/votings/{votingId}/targets/{unitId}");

        await EnsureSuccessAsync(response);

        Assert.False(await TargetExistsAsync(votingId, unitId));
    }

    [Fact]
    public async Task GetTargets_Should_Return_NotFound_When_Voting_Does_Not_Exist()
    {
        var missingVotingId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/votings/{missingVotingId}/targets");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostTargetAsync(Guid votingId, Guid educationUnitId) =>
        _client.PostAsJsonAsync(
            $"/api/votings/{votingId}/targets",
            new
            {
                votingId = votingId.ToString(),
                educationUnitId = educationUnitId.ToString()
            });

    private async Task<(Guid VotingId, Guid EducationUnitId)> SeedVotingAndEducationUnitAsync()
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

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
                Title = "Target voting",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddMinutes(5),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = DbVotingStatus.Draft,
                CreatedById = userId
            });

            dbContext.EducationUnits.Add(new EducationUnit
            {
                Id = unitId,
                Name = "Target group",
                Type = EducationUnitType.Group
            });

            await dbContext.SaveChangesAsync();
        });

        return (votingId, unitId);
    }

    private async Task<Guid> SeedVotingWithTargetAsync(Guid educationUnitId)
    {
        var votingId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

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
                Title = "Target voting",
                Description = "Description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddMinutes(5),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = DbVotingStatus.Draft,
                CreatedById = userId
            });

            dbContext.EducationUnits.Add(new EducationUnit
            {
                Id = educationUnitId,
                Name = "Target group",
                Type = EducationUnitType.Group
            });

            dbContext.VotingTargets.Add(new VotingTarget
            {
                VotingId = votingId,
                EducationUnitId = educationUnitId
            });

            await dbContext.SaveChangesAsync();
        });

        return votingId;
    }

    private Task<bool> TargetExistsAsync(Guid votingId, Guid educationUnitId)
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.VotingTargets.AnyAsync(t =>
                t.VotingId == votingId && t.EducationUnitId == educationUnitId));
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private sealed record VotingTargetApiResponse(
        [property: JsonPropertyName("votingId")] string VotingId,
        [property: JsonPropertyName("educationUnitId")] string EducationUnitId);

    private sealed record GetVotingTargetsApiResponse(
        [property: JsonPropertyName("targets")] List<VotingTargetApiResponse> Targets);
}
