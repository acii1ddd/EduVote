using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduVote.Tests.Auth;

public class AuthIntegrationTests(EduVoteApiFactory factory)
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

    [Fact(DisplayName = "Регистрация: создаёт пользователя с ролью Student")]
    public async Task Register_Should_Create_Student_User()
    {
        var response = await PostRegisterAsync("newuser@example.com", "password123", "New User");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<RegisterApiResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.UserId));

        var user = await GetUserByEmailAsync("newuser@example.com");
        Assert.Equal("New User", user.Name);
        Assert.Equal(Roles.Student, user.UserRole.Name);
    }

    [Fact(DisplayName = "Регистрация: возвращает конфликт, если email уже занят")]
    public async Task Register_Should_Return_Conflict_When_Email_Already_Exists()
    {
        await PostRegisterAsync("dup@example.com", "password123", "First User");

        var response = await PostRegisterAsync("dup@example.com", "password456", "Second User");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal("Пользователь с таким email уже существует", body.Message);
    }

    [Fact(DisplayName = "Вход: возвращает токен при корректных учётных данных")]
    public async Task Login_Should_Return_Token_For_Valid_Credentials()
    {
        await PostRegisterAsync("login@example.com", "password123", "Login User");

        var response = await PostLoginAsync("login@example.com", "password123");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<LoginApiResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.UserId));
        Assert.Equal(Roles.Student, body.Role);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
    }

    [Fact(DisplayName = "Вход: возвращает ошибку при неверном пароле")]
    public async Task Login_Should_Return_BadRequest_For_Invalid_Password()
    {
        await PostRegisterAsync("wrongpass@example.com", "password123", "User");

        var response = await PostLoginAsync("wrongpass@example.com", "wrong-password");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private Task<HttpResponseMessage> PostRegisterAsync(string email, string password, string name) =>
        _client.PostAsJsonAsync("/api/auth/register", new { email, password, name });

    private Task<HttpResponseMessage> PostLoginAsync(string email, string password) =>
        _client.PostAsJsonAsync("/api/auth/login", new { email, password });

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

    private Task<User> GetUserByEmailAsync(string email)
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRole)
                .SingleAsync(u => u.Email == email));
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private sealed record RegisterApiResponse(
        [property: JsonPropertyName("userId")] string UserId);

    private sealed record ApiErrorResponse(
        [property: JsonPropertyName("message")] string Message);

    private sealed record LoginApiResponse(
        [property: JsonPropertyName("userId")] string UserId,
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("accessToken")] string AccessToken);
}
