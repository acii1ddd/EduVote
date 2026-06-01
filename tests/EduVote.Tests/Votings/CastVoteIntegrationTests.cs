using System.Net.Http.Json;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DbEducationUnitType = EduVote.DAL.Postgresql.Models.Enums.EducationUnitType;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class CastVoteIntegrationTests(EduVoteApiFactory factory)
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
    public async Task CastVote_Should_Create_SingleChoice_Vote()
    {
        var scenario = await SeedSingleChoiceScenarioAsync(allowVoteChange: false);
        AuthorizeAs(scenario.UserId);

        var response = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.FirstCandidateId);

        await EnsureSuccessAsync(response);

        var votes = await GetVotesAsync(scenario.VotingId, scenario.UserId);
        var vote = Assert.Single(votes);

        Assert.Equal(scenario.FirstCandidateId, vote.CandidateId);
        Assert.False(string.IsNullOrWhiteSpace(vote.VoteHash));
        Assert.False(string.IsNullOrWhiteSpace(vote.VoteSalt));
    }

    [Fact]
    public async Task CastVote_Should_Reject_Second_Vote_When_Change_Is_Not_Allowed()
    {
        var scenario = await SeedSingleChoiceScenarioAsync(allowVoteChange: false);
        AuthorizeAs(scenario.UserId);

        var firstResponse = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.FirstCandidateId);
        await EnsureSuccessAsync(firstResponse);

        var secondResponse = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.SecondCandidateId);

        Assert.False(secondResponse.IsSuccessStatusCode);

        var votes = await GetVotesAsync(scenario.VotingId, scenario.UserId);
        var vote = Assert.Single(votes);

        Assert.Equal(scenario.FirstCandidateId, vote.CandidateId);
    }

    [Fact]
    public async Task CastVote_Should_Update_Existing_Vote_When_Change_Is_Allowed()
    {
        var scenario = await SeedSingleChoiceScenarioAsync(allowVoteChange: true);
        AuthorizeAs(scenario.UserId);

        var firstResponse = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.FirstCandidateId);
        await EnsureSuccessAsync(firstResponse);

        var firstVote = Assert.Single(await GetVotesAsync(scenario.VotingId, scenario.UserId));

        var secondResponse = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.SecondCandidateId);
        await EnsureSuccessAsync(secondResponse);

        var votes = await GetVotesAsync(scenario.VotingId, scenario.UserId);
        var updatedVote = Assert.Single(votes);

        Assert.Equal(firstVote.Id, updatedVote.Id);
        Assert.Equal(scenario.SecondCandidateId, updatedVote.CandidateId);
        Assert.NotEqual(firstVote.VoteHash, updatedVote.VoteHash);
        Assert.NotEqual(firstVote.VoteSalt, updatedVote.VoteSalt);
    }

    [Fact]
    public async Task CastVote_Should_Reject_Vote_When_Voting_Is_Finished()
    {
        var scenario = await SeedSingleChoiceScenarioAsync(
            allowVoteChange: false,
            status: DbVotingStatus.Finished);
        AuthorizeAs(scenario.UserId);

        var response = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.FirstCandidateId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.Empty(await GetVotesAsync(scenario.VotingId, scenario.UserId));
    }

    [Fact]
    public async Task CastVote_Should_Reject_Vote_When_Voting_Is_Paused()
    {
        var scenario = await SeedSingleChoiceScenarioAsync(
            allowVoteChange: false,
            status: DbVotingStatus.Paused);
        AuthorizeAs(scenario.UserId);

        var response = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.FirstCandidateId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.Empty(await GetVotesAsync(scenario.VotingId, scenario.UserId));
    }

    [Fact]
    public async Task CastVote_Should_Reject_Vote_When_User_Is_Not_In_Target_Education_Unit()
    {
        var scenario = await SeedSingleChoiceScenarioAsync(
            allowVoteChange: false,
            restrictToDifferentEducationUnit: true);
        AuthorizeAs(scenario.UserId);

        var response = await PostSingleChoiceVoteAsync(scenario.VotingId, scenario.FirstCandidateId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.Empty(await GetVotesAsync(scenario.VotingId, scenario.UserId));
    }

    [Fact]
    public async Task CastVote_Should_Reject_Vote_When_Candidate_Is_Not_In_Voting()
    {
        var scenario = await SeedSingleChoiceScenarioAsync(allowVoteChange: false);
        AuthorizeAs(scenario.UserId);

        var response = await PostSingleChoiceVoteAsync(scenario.VotingId, Guid.NewGuid());

        Assert.False(response.IsSuccessStatusCode);
        Assert.Empty(await GetVotesAsync(scenario.VotingId, scenario.UserId));
    }

    private void AuthorizeAs(Guid userId)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, Roles.Student);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private Task<HttpResponseMessage> PostSingleChoiceVoteAsync(Guid votingId, Guid candidateId)
    {
        return _client.PostAsJsonAsync($"/api/votings/{votingId}/vote", new
        {
            votingId = votingId.ToString(),
            selectedCandidateId = candidateId.ToString()
        });
    }

    private Task<List<Vote>> GetVotesAsync(Guid votingId, Guid userId)
    {
        return factory.ExecuteDbContextAsync(dbContext =>
            dbContext.Votes
                .AsNoTracking()
                .Where(v => v.VotingId == votingId && v.UserId == userId)
                .ToListAsync());
    }

    private async Task<SingleChoiceScenario> SeedSingleChoiceScenarioAsync(
        bool allowVoteChange,
        DbVotingStatus status = DbVotingStatus.Active,
        bool restrictToDifferentEducationUnit = false)
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var votingId = Guid.NewGuid();
        var firstCandidateId = Guid.NewGuid();
        var secondCandidateId = Guid.NewGuid();
        var userEducationUnitId = Guid.NewGuid();
        var targetEducationUnitId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role
            {
                Id = roleId,
                Name = Roles.Student
            });

            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"student-{userId:N}@example.com",
                Name = "Student",
                PasswordHash = "hash",
                RoleId = roleId
            });

            if (restrictToDifferentEducationUnit)
            {
                dbContext.EducationUnits.AddRange(
                    new EducationUnit
                    {
                        Id = userEducationUnitId,
                        Name = $"User group {userEducationUnitId:N}",
                        Type = DbEducationUnitType.Group
                    },
                    new EducationUnit
                    {
                        Id = targetEducationUnitId,
                        Name = $"Target group {targetEducationUnitId:N}",
                        Type = DbEducationUnitType.Group
                    });

                dbContext.UserEducationUnits.Add(new UserEducationUnit
                {
                    UserId = userId,
                    EducationUnitId = userEducationUnitId
                });
            }

            dbContext.Votings.Add(new Voting
            {
                Id = votingId,
                Title = "Integration voting",
                Description = "Single choice voting for integration tests",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = allowVoteChange,
                StartTime = DateTime.UtcNow.AddMinutes(-5),
                EndTime = DateTime.UtcNow.AddHours(1),
                Status = status,
                CreatedById = userId
            });

            dbContext.Candidates.AddRange(
                new Candidate
                {
                    Id = firstCandidateId,
                    VotingId = votingId,
                    Name = "First candidate",
                    Description = "First candidate description"
                },
                new Candidate
                {
                    Id = secondCandidateId,
                    VotingId = votingId,
                    Name = "Second candidate",
                    Description = "Second candidate description"
                });

            if (restrictToDifferentEducationUnit)
            {
                dbContext.VotingTargets.Add(new VotingTarget
                {
                    VotingId = votingId,
                    EducationUnitId = targetEducationUnitId
                });
            }

            await dbContext.SaveChangesAsync();
        });

        return new SingleChoiceScenario(userId, votingId, firstCandidateId, secondCandidateId);
    }

    private sealed record SingleChoiceScenario(
        Guid UserId,
        Guid VotingId,
        Guid FirstCandidateId,
        Guid SecondCandidateId);
}
