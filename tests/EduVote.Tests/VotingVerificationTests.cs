using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace EduVote.Tests;

public class VotingVerificationTests(ITestOutputHelper output)
{
    [Fact]
    public void Should_Hash_Guid_With_Sha256()
    {
        // Arrange
        var guid = Guid.Parse("3c0f622e-6476-4b3f-8727-bcf8a960ce11");
        
        // Act
        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(guid.ToString())));
        
        // Output
        output.WriteLine($"Guid: {guid}");
        output.WriteLine($"SHA256: {hash}");
        
        // Assert
        Assert.NotNull(hash);
        Assert.Equal(64, hash.Length);
    }
}