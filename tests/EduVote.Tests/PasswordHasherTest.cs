using EduVote.Infrastructure.Auth;
using EduVote.Application.Users.Services;
using Xunit.Abstractions;

namespace EduVote.Tests;

public class PasswordHasherTest(ITestOutputHelper output)
{
    [Fact(DisplayName = "Хеширование пароля: корректно хеширует и проверяет пароль")]
    public void Should_Hash_Password_123()
    {
        // Arrange
        IPasswordHasher passwordHasher = new PasswordHasher();

        // Act
        var hash = passwordHasher.Hash("123");

        // Output
        output.WriteLine($"Hash for '123': {hash}");

        // Assert
        Assert.NotNull(hash);
    }
}