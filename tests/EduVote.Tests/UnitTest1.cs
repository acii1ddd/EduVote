using EduVote.API.Services.Auth.PasswordHasher;
using Xunit.Abstractions;

namespace EduVote.Tests;

public class UnitTest1(ITestOutputHelper output)
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