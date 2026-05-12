using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace EduVote.API.Services.Tools;

public class BlockchainService
{
    private readonly Web3 _web3;
    private readonly string _accountAddress;
    private readonly ILogger<BlockchainService> _logger;

    public BlockchainService(
        IConfiguration configuration, 
        ILogger<BlockchainService> logger)
    {
        var metamaskPrivateKey = configuration["Blockchain:MetaMaskPrivateKey"];
        var infuraUrl  = configuration["Blockchain:InfuraUrl"];
        
        var account = new Account(metamaskPrivateKey, Chain.Sepolia);
        _web3 = new Web3(account, infuraUrl);
        _accountAddress = account.Address;
        _logger = logger;
    }

    public async Task<string> WriteResultHashAsync(string resultHash)
    {
        try
        {
            var data = "0x" + Convert.ToHexString(Encoding.UTF8.GetBytes(resultHash));

            _logger.LogInformation("Sending transaction with data: {Data}", data);
        
            var transactionInput = new TransactionInput
            {
                From = _accountAddress,
                To = _accountAddress,
                Value = new HexBigInteger(0),
                Data = data,
                Gas = new HexBigInteger(100000)
            };

            var txHash = await _web3.Eth.TransactionManager
                .SendTransactionAsync(transactionInput);

            _logger.LogInformation("Transaction sent. TxHash: {TxHash}", txHash);
            _logger.LogInformation("Waiting for receipt...");
        
            // wait for transaction receipt to confirm the transaction
            var receipt = await _web3.Eth.TransactionManager.TransactionReceiptService
                .PollForReceiptAsync(txHash);
        
            if (receipt == null || receipt.Status.Value == 0)
                throw new Exception($"Blockchain transaction failed. TxHash: {txHash}");
        
            _logger.LogInformation("Transaction confirmed! Block: {Block}", receipt.BlockNumber);
            _logger.LogInformation("Etherscan: https://sepolia.etherscan.io/tx/{TxHash}", txHash);
        
            return txHash;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error with blockchain operation");
            throw;
        }
    }
    
    // var txHash = await _blockchainService.WriteResultHashAsync(resultHash)
}