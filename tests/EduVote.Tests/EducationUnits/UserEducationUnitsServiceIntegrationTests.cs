using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Enums;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduVote.Tests.EducationUnits;

public class UserEducationUnitsServiceIntegrationTests(EduVoteApiFactory factory)
    : IClassFixture<EduVoteApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    private Guid _userId;
    private Guid _educationUnitId;

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
        (_userId, _educationUnitId) = await SeedUserAndEducationUnitAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetEducationUnits_Should_Return_Seeded_Unit()
    {
        var response = await _client.GetAsync("/api/education-units");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetEducationUnitsApiResponse>();
        Assert.NotNull(body);
        Assert.Single(body.EducationUnits);

        var unit = body.EducationUnits[0];
        Assert.Equal(_educationUnitId.ToString(), unit.Id);
        Assert.Equal("Test Group", unit.Name);
        Assert.Equal(EducationUnitType.Group.ToString(), unit.Type);
    }

    [Fact]
    public async Task AssignUserToEducationUnit_Should_Create_Relation_In_Database()
    {
        var secondUnitId = Guid.NewGuid();
        await SeedEducationUnitAsync(secondUnitId, "Second Group");

        var response = await _client.PostAsync(
            $"/api/users/{_userId}/education-units/{secondUnitId}",
            null);

        await EnsureSuccessAsync(response);

        var exists = await factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.UserEducationUnits.AnyAsync(ueu =>
                ueu.UserId == _userId && ueu.EducationUnitId == secondUnitId));

        Assert.True(exists);
    }

    [Fact]
    public async Task RemoveUserFromEducationUnit_Should_Delete_Relation()
    {
        var response = await _client.DeleteAsync(
            $"/api/users/{_userId}/education-units/{_educationUnitId}");

        await EnsureSuccessAsync(response);

        var exists = await factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.UserEducationUnits.AnyAsync(ueu =>
                ueu.UserId == _userId && ueu.EducationUnitId == _educationUnitId));

        Assert.False(exists);
    }

    [Fact]
    public async Task AssignUserToEducationUnit_Should_Be_Idempotent_When_Relation_Already_Exists()
    {
        var response = await _client.PostAsync(
            $"/api/users/{_userId}/education-units/{_educationUnitId}",
            null);

        await EnsureSuccessAsync(response);
    }

    [Fact]
    public async Task AssignUserToEducationUnit_Should_Replace_Previous_Unit()
    {
        var newUnitId = Guid.NewGuid();
        await SeedEducationUnitAsync(newUnitId, "Replacement Group");

        var response = await _client.PostAsync(
            $"/api/users/{_userId}/education-units/{newUnitId}",
            null);

        await EnsureSuccessAsync(response);

        var units = await GetUserEducationUnitIdsAsync(_userId);

        Assert.Single(units);
        Assert.Equal(newUnitId, units[0]);
    }

    [Fact]
    public async Task AssignUserToEducationUnit_Should_Collapse_Multiple_Units_To_One()
    {
        var secondUnitId = Guid.NewGuid();
        await SeedEducationUnitAsync(secondUnitId, "Second Group");

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.UserEducationUnits.Add(new UserEducationUnit
            {
                UserId = _userId,
                EducationUnitId = secondUnitId
            });

            await dbContext.SaveChangesAsync();
        });

        var response = await _client.PostAsync(
            $"/api/users/{_userId}/education-units/{_educationUnitId}",
            null);

        await EnsureSuccessAsync(response);

        var units = await GetUserEducationUnitIdsAsync(_userId);

        Assert.Single(units);
        Assert.Equal(_educationUnitId, units[0]);
    }

    [Fact]
    public async Task AssignUserToEducationUnit_Should_Return_InvalidArgument_For_Invalid_User_Id()
    {
        var response = await _client.PostAsync(
            "/api/users/not-a-guid/education-units/" + _educationUnitId,
            null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(Guid UserId, Guid EducationUnitId)> SeedUserAndEducationUnitAsync()
    {
        var userId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            var studentRole = await dbContext.Roles
                .SingleOrDefaultAsync(r => r.Name == Roles.Student);

            if (studentRole is null)
            {
                studentRole = new Role { Id = Guid.NewGuid(), Name = Roles.Student };
                dbContext.Roles.Add(studentRole);
            }

            dbContext.Users.Add(new User
            {
                Id = userId,
                Email = "unit-user@example.com",
                Name = "Unit User",
                RoleId = studentRole.Id,
                PasswordHash = "hash"
            });

            dbContext.EducationUnits.Add(new EducationUnit
            {
                Id = unitId,
                Name = "Test Group",
                Type = EducationUnitType.Group
            });

            dbContext.UserEducationUnits.Add(new UserEducationUnit
            {
                UserId = userId,
                EducationUnitId = unitId
            });

            await dbContext.SaveChangesAsync();
        });

        return (userId, unitId);
    }

    private Task<List<Guid>> GetUserEducationUnitIdsAsync(Guid userId) =>
        factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.UserEducationUnits
                .Where(ueu => ueu.UserId == userId)
                .Select(ueu => ueu.EducationUnitId)
                .ToListAsync());

    private Task SeedEducationUnitAsync(Guid unitId, string name)
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.EducationUnits.Add(new EducationUnit
            {
                Id = unitId,
                Name = name,
                Type = EducationUnitType.Group
            });

            await dbContext.SaveChangesAsync();
        });
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private sealed record EducationUnitApiResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("parentId")] string? ParentId);

    private sealed record GetEducationUnitsApiResponse(
        [property: JsonPropertyName("educationUnits")] List<EducationUnitApiResponse> EducationUnits);
}
