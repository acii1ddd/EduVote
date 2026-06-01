using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduVote.Tests.RolesCatalog;

public class RoleServiceIntegrationTests(EduVoteApiFactory factory)
    : IClassFixture<EduVoteApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
        await SeedRolesAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetRoles_Should_Return_All_Seeded_Roles()
    {
        var response = await _client.GetAsync("/api/roles");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetRolesApiResponse>();
        Assert.NotNull(body);

        var names = body.Roles.Select(r => r.Name).ToHashSet();
        Assert.Contains(Roles.Student, names);
        Assert.Contains(Roles.Teacher, names);
        Assert.Contains(Roles.Administrator, names);
        Assert.Equal(3, body.Roles.Count);
    }

    private async Task SeedRolesAsync()
    {
        await factory.ExecuteDbContextAsync(async dbContext =>
        {
            if (await dbContext.Roles.AnyAsync())
                return;

            dbContext.Roles.AddRange(
                new Role { Id = Guid.NewGuid(), Name = Roles.Student },
                new Role { Id = Guid.NewGuid(), Name = Roles.Teacher },
                new Role { Id = Guid.NewGuid(), Name = Roles.Administrator });

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

    private sealed record RoleApiResponse(
        [property: JsonPropertyName("name")] string Name);

    private sealed record GetRolesApiResponse(
        [property: JsonPropertyName("roles")] List<RoleApiResponse> Roles);
}
