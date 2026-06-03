using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using DbEducationUnitType = EduVote.DAL.Postgresql.Models.Enums.EducationUnitType;
using DbVotingStatus = EduVote.DAL.Postgresql.Models.Enums.VotingStatus;
using DbVotingType = EduVote.DAL.Postgresql.Models.Enums.VotingType;

namespace EduVote.Tests.Votings;

public class GetUserVotingsIntegrationTests(EduVoteApiFactory factory)
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

    [Fact(DisplayName = "Голосования пользователя: 404 для несуществующего пользователя")]
    public async Task GetVotingsForUser_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        var missingUserId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/users/{missingUserId}/votings");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Голосования пользователя: публичные и по таргету")]
    public async Task GetVotingsForUser_Should_Return_Public_And_Matching_Targeted_Votings()
    {
        var userUnitId = Guid.NewGuid();
        var otherUnitId = Guid.NewGuid();
        var userId = await SeedUserWithEducationUnitAsync(userUnitId);

        var publicVotingId = await SeedVotingAsync("Public voting", targets: []);
        var matchingVotingId = await SeedVotingAsync("Matching voting", targets: [userUnitId]);
        await SeedVotingAsync("Other unit voting", targets: [otherUnitId]);

        var response = await _client.GetAsync($"/api/users/{userId}/votings");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotingsApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Votings.Count);

        var titles = body.Votings.Select(v => v.Title).ToHashSet();
        Assert.Contains("Public voting", titles);
        Assert.Contains("Matching voting", titles);
        Assert.DoesNotContain("Other unit voting", titles);

        Assert.Contains(body.Votings, v => v.Id == publicVotingId.ToString());
        Assert.Contains(body.Votings, v => v.Id == matchingVotingId.ToString());
    }

    [Fact(DisplayName = "Голосования пользователя: только публичные без учебной единицы")]
    public async Task GetVotingsForUser_Should_Return_Only_Public_Votings_When_User_Has_No_Education_Units()
    {
        var userId = await SeedUserWithoutEducationUnitsAsync();
        var publicVotingId = await SeedVotingAsync("Public voting", targets: []);
        await SeedVotingAsync("Restricted voting", targets: [Guid.NewGuid()]);

        var response = await _client.GetAsync($"/api/users/{userId}/votings");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotingsApiResponse>();
        Assert.NotNull(body);
        Assert.Single(body.Votings);
        Assert.Equal(publicVotingId.ToString(), body.Votings[0].Id);
        Assert.Equal("Public voting", body.Votings[0].Title);
    }

    [Fact(DisplayName = "Голосования пользователя: учитывает таргет родительской единицы")]
    public async Task GetVotingsForUser_Should_Include_Voting_Targeted_At_Parent_Unit()
    {
        var parentUnitId = Guid.NewGuid();
        var childUnitId = Guid.NewGuid();
        var userId = await SeedUserWithEducationUnitAsync(childUnitId, parentUnitId);

        var parentTargetVotingId = await SeedVotingAsync(
            "Parent targeted voting",
            targets: [parentUnitId]);

        var response = await _client.GetAsync($"/api/users/{userId}/votings");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotingsApiResponse>();
        Assert.NotNull(body);
        Assert.Contains(body.Votings, v => v.Id == parentTargetVotingId.ToString());
    }

    [Fact(DisplayName = "Созданные мной: только голосования текущего пользователя")]
    public async Task GetVotingsCreatedByUser_Should_Return_Only_Votings_Created_By_Current_User()
    {
        var teacherId = await SeedUserWithoutEducationUnitsAsync(Roles.Teacher);
        var otherTeacherId = await SeedUserWithoutEducationUnitsAsync(Roles.Teacher);

        var ownVotingId = await SeedVotingAsync("Own voting", createdById: teacherId);
        await SeedVotingAsync("Other teacher voting", createdById: otherTeacherId);

        AuthorizeAs(teacherId, Roles.Teacher);

        var response = await _client.GetAsync("/api/votings/created");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetVotingsApiResponse>();
        Assert.NotNull(body);
        Assert.Single(body.Votings);
        Assert.Equal(ownVotingId.ToString(), body.Votings[0].Id);
        Assert.Equal("Own voting", body.Votings[0].Title);
    }

    [Fact(DisplayName = "Созданные мной: 401 без авторизации")]
    public async Task GetVotingsCreatedByUser_Should_Return_Unauthenticated_Without_Auth_Headers()
    {
        var response = await _client.GetAsync("/api/votings/created");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private void AuthorizeAs(Guid userId, string role)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.RoleHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
    }

    private async Task<Guid> SeedUserWithoutEducationUnitsAsync(string roleName = Roles.Student)
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role { Id = roleId, Name = roleName });
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

    private async Task<Guid> SeedUserWithEducationUnitAsync(
        Guid educationUnitId,
        Guid? parentUnitId = null)
    {
        var roleId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Roles.Add(new Role { Id = roleId, Name = Roles.Student });

            if (parentUnitId is not null)
            {
                dbContext.EducationUnits.Add(new EducationUnit
                {
                    Id = parentUnitId.Value,
                    Name = $"Parent {parentUnitId.Value:N}",
                    Type = DbEducationUnitType.Faculty
                });
            }

            dbContext.EducationUnits.Add(new EducationUnit
            {
                Id = educationUnitId,
                Name = $"Unit {educationUnitId:N}",
                Type = DbEducationUnitType.Group,
                ParentId = parentUnitId
            });

            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = $"student-{userId:N}@example.com",
                Name = "Student",
                PasswordHash = "hash",
                RoleId = roleId
            });

            dbContext.UserEducationUnits.Add(new UserEducationUnit
            {
                UserId = userId,
                EducationUnitId = educationUnitId
            });

            await dbContext.SaveChangesAsync();
        });

        return userId;
    }

    private async Task<Guid> SeedVotingAsync(
        string title,
        Guid? createdById = null,
        IReadOnlyList<Guid>? targets = null)
    {
        var roleId = Guid.NewGuid();
        var creatorId = createdById ?? Guid.NewGuid();
        var votingId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            if (createdById is null)
            {
                dbContext.Roles.Add(new Role { Id = roleId, Name = Roles.Administrator });
                dbContext.Users.Add(new User
                {
                    Id = creatorId,
                    Email = $"admin-{creatorId:N}@example.com",
                    Name = "Administrator",
                    PasswordHash = "hash",
                    RoleId = roleId
                });
            }

            dbContext.Votings.Add(new Voting
            {
                Id = votingId,
                Title = title,
                Description = "Test description",
                Type = DbVotingType.SingleChoice,
                IsAnonymous = false,
                AllowVoteChange = false,
                StartTime = DateTime.UtcNow.AddMinutes(5),
                EndTime = DateTime.UtcNow.AddHours(2),
                Status = DbVotingStatus.Draft,
                CreatedById = creatorId
            });

            if (targets is { Count: > 0 })
            {
                foreach (var unitId in targets.Distinct())
                {
                    if (!dbContext.EducationUnits.Any(e => e.Id == unitId))
                    {
                        dbContext.EducationUnits.Add(new EducationUnit
                        {
                            Id = unitId,
                            Name = $"Unit {unitId:N}",
                            Type = DbEducationUnitType.Group
                        });
                    }

                    dbContext.VotingTargets.Add(new VotingTarget
                    {
                        VotingId = votingId,
                        EducationUnitId = unitId
                    });
                }
            }

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

    private sealed record VotingListItem(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("title")] string Title);

    private sealed record GetVotingsApiResponse(
        [property: JsonPropertyName("votings")] List<VotingListItem> Votings);
}
