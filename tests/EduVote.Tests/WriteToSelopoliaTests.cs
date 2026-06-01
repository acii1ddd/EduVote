using EduVote.API.Services.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace EduVote.Tests;

public class WriteToSepoliaTests(ITestOutputHelper output)
{
    [Fact]
    public async Task WriteResultHashAsync_ShouldReturnTxHash()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });
        
        var logger = loggerFactory.CreateLogger<BlockchainService>();
        
        var service = new BlockchainService(configuration, logger);
        const string testHash = "a3f8c2d1e4b5a6f7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1";
        
        // Act
        var (txHash, blockNumber) = await service.WriteResultHashAsync(testHash);
        
        // Assert
        Assert.NotNull(txHash);
        Assert.StartsWith("0x", txHash);
        
        output.WriteLine($"TxHash: {txHash}, BlockNumber: {blockNumber}");
        output.WriteLine($"Etherscan: https://sepolia.etherscan.io/tx/{txHash}");
    }
}