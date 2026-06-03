using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EduVote.DAL.Postgresql.Models;
using EduVote.DAL.Postgresql.Models.Roles;
using EduVote.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduVote.Tests.Users;

public class UserServiceIntegrationTests(EduVoteApiFactory factory)
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

    [Fact(DisplayName = "Пользователи: создание студента")]
    public async Task CreateUser_Should_Create_Student_User()
    {
        var response = await PostCreateUserAsync(
            "student@example.com",
            "password123",
            "Student User");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<UserApiResponse>();
        Assert.NotNull(body);
        Assert.Equal("student@example.com", body.Email);
        Assert.Equal("Student User", body.Name);
        Assert.Equal(Roles.Student, body.Role);
        Assert.False(string.IsNullOrWhiteSpace(body.Id));
    }

    [Fact(DisplayName = "Пользователи: конфликт при дублировании email")]
    public async Task CreateUser_Should_Return_AlreadyExists_When_Email_Is_Duplicated()
    {
        await PostCreateUserAsync("dup@example.com", "password123", "First");
        var response = await PostCreateUserAsync("dup@example.com", "password456", "Second");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal("Пользователь с таким email уже существует", body.Message);
    }

    [Fact(DisplayName = "Пользователи: список включает созданных")]
    public async Task GetUsers_Should_Return_Created_Users()
    {
        await PostCreateUserAsync("one@example.com", "password123", "User One");
        await PostCreateUserAsync("two@example.com", "password123", "User Two");

        var response = await _client.GetAsync("/api/users");

        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadFromJsonAsync<GetUsersApiResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Users.Count);
        Assert.Contains(body.Users, u => u.Email == "one@example.com");
        Assert.Contains(body.Users, u => u.Email == "two@example.com");
    }

    [Fact(DisplayName = "Пользователи: обновление имени и email")]
    public async Task UpdateUser_Should_Update_Name_And_Email()
    {
        var createResponse = await PostCreateUserAsync(
            "update@example.com",
            "password123",
            "Before Update");
        await EnsureSuccessAsync(createResponse);

        var created = await createResponse.Content.ReadFromJsonAsync<UserApiResponse>();
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{created.Id}", new
        {
            id = created.Id,
            email = "updated@example.com",
            name = "After Update",
            role = Roles.Teacher
        });

        await EnsureSuccessAsync(updateResponse);

        var user = await GetUserFromDbAsync(Guid.Parse(created.Id));
        Assert.Equal("updated@example.com", user.Email);
        Assert.Equal("After Update", user.Name);
        Assert.Equal(Roles.Teacher, user.UserRole.Name);
    }

    [Fact(DisplayName = "Пользователи: обновление отклоняет пустые обязательные поля")]
    public async Task UpdateUser_Should_Return_InvalidArgument_When_Required_Fields_Are_Empty()
    {
        var createResponse = await PostCreateUserAsync(
            "empty-update@example.com",
            "password123",
            "User");
        await EnsureSuccessAsync(createResponse);

        var created = await createResponse.Content.ReadFromJsonAsync<UserApiResponse>();
        Assert.NotNull(created);

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{created.Id}", new
        {
            id = created.Id,
            email = "   ",
            name = "   ",
            role = Roles.Student
        });

        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        var body = await updateResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal("Заполните все обязательные поля", body.Message);
    }

    [Fact(DisplayName = "Пользователи: обновление возвращает конфликт при дублировании email")]
    public async Task UpdateUser_Should_Return_AlreadyExists_When_Email_Is_Duplicated()
    {
        var firstResponse = await PostCreateUserAsync("first@example.com", "password123", "First");
        var secondResponse = await PostCreateUserAsync("second@example.com", "password123", "Second");
        await EnsureSuccessAsync(firstResponse);
        await EnsureSuccessAsync(secondResponse);

        var second = await secondResponse.Content.ReadFromJsonAsync<UserApiResponse>();
        Assert.NotNull(second);

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{second.Id}", new
        {
            id = second.Id,
            email = "first@example.com",
            name = "Second",
            role = Roles.Student
        });

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);
        var body = await updateResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal("Пользователь с таким email уже существует", body.Message);
    }

    private Task<HttpResponseMessage> PostCreateUserAsync(string email, string password, string name)
    {
        return _client.PostAsJsonAsync("/api/users", new
        {
            email,
            password,
            name,
            role = Roles.Student
        });
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

    private Task<User> GetUserFromDbAsync(Guid userId)
    {
        return factory.ExecuteDbContextAsync(async dbContext =>
            await dbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRole)
                .SingleAsync(u => u.Id == userId));
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        Assert.Fail($"Expected success status code, got {(int)response.StatusCode} {response.ReasonPhrase}. Body: {content}");
    }

    private sealed record UserApiResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("role")] string Role);

    private sealed record GetUsersApiResponse(
        [property: JsonPropertyName("users")] List<UserApiResponse> Users);

    private sealed record ApiErrorResponse(
        [property: JsonPropertyName("message")] string Message);
}
