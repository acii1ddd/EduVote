namespace EduVote.Application.Votings.Services;

public interface IBlockchainResultWriter
{
    Task<(string TxHash, string BlockNumber)> WriteResultHashAsync(string resultHash);
}
