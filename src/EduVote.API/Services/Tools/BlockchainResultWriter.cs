using EduVote.Application.Votings.Services;

namespace EduVote.API.Services.Tools;

public sealed class BlockchainResultWriter(BlockchainService blockchainService) : IBlockchainResultWriter
{
    public async Task<(string TxHash, string BlockNumber)> WriteResultHashAsync(string resultHash)
    {
        var (txHash, blockNumber) = await blockchainService.WriteResultHashAsync(resultHash);

        return (txHash, blockNumber.ToString());
    }
}
