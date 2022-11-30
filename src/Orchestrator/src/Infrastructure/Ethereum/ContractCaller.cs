using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Nethereum.Web3;

using Application.Common.Interfaces;

using Infrastructure.Ethereum.Messages;

namespace Infrastructure.Ethereum;

internal class ContractCaller : IContractCaller
{
    private readonly ILogger<ContractCaller> _logger;
    private readonly Web3 _web3;
    private readonly string _verifierLotteryAddress;

    public ContractCaller(ILogger<ContractCaller> logger, IConfiguration configuration)
    {
        _logger = logger;

        var network = configuration["Ethereum:Network"]!;
        var orchestrator = new Nethereum.Web3.Accounts.Account(
            configuration[$"Ethereum:Accounts:{network}:Orchestrator:PrivateKey"]!,
            configuration.GetValue<int>($"Ethereum:Networks:{network}:ChainId")
        );
        _web3 = new Web3(orchestrator, configuration[$"Ethereum:Networks:{network}:URL"]);
        _verifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!;
    }

    public Task<byte[]> ComputeHash(byte[] data)
    {
        var queryDispatcher = _web3.Eth.GetContractQueryHandler<ComputeHashMessage>();
        return queryDispatcher.QueryAsync<byte[]>(_verifierLotteryAddress, new ComputeHashMessage { Data = data });
    }

    public async Task InitVerifierLottery(string thingId, byte[] dataHash)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<InitVerifierLotteryMessage>();
        try
        {
            var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                _verifierLotteryAddress,
                new InitVerifierLotteryMessage
                {
                    ThingId = thingId,
                    DataHash = dataHash
                }
            );

            _logger.LogInformation(txnReceipt.LogsBloom);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}