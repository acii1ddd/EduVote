using EduVote.API.Services.Auth.PasswordHasher;
using EduVote.Application.Users.Services;
using Xunit.Abstractions;

namespace EduVote.Tests;

public class PasswordHasherTest(ITestOutputHelper output)
{
    [Fact]
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