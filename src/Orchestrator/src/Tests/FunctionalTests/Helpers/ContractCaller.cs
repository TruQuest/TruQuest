using System.Numerics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

using Application.Thing.Commands.SubmitNewThing;
using Infrastructure.Ethereum.TypedData;

using Tests.FunctionalTests.Helpers.Messages;

namespace Tests.FunctionalTests.Helpers;

public class ContractCaller
{
    private readonly ILogger _logger;
    private readonly BlockchainManipulator _blockchainManipulator;

    private readonly Web3 _web3;
    private readonly string _rpcUrl;
    private readonly string _truthserumAddress;
    private readonly string _truQuestAddress;
    private readonly string _verifierLotteryAddress;
    private readonly string _acceptancePollAddress;

    private readonly Account _orchestrator;

    public ContractCaller(ILogger logger, IConfiguration configuration, BlockchainManipulator blockchainManipulator)
    {
        _logger = logger;
        _blockchainManipulator = blockchainManipulator;

        var network = configuration["Ethereum:Network"]!;
        var player = new Account(
            configuration[$"Ethereum:Accounts:{network}:Player:PrivateKey"]!,
            configuration.GetValue<int>($"Ethereum:Networks:{network}:ChainId")
        );
        _rpcUrl = configuration[$"Ethereum:Networks:{network}:URL"]!;
        _web3 = new Web3(player, _rpcUrl);
        _truthserumAddress = configuration[$"Ethereum:Contracts:{network}:Truthserum:Address"]!;
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
        _verifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;

        _orchestrator = new Account(
            configuration[$"Ethereum:Accounts:{network}:Orchestrator:PrivateKey"]!,
            configuration.GetValue<int>($"Ethereum:Networks:{network}:ChainId")
        );
    }

    public async Task TransferTruthserumTo(string address, BigInteger amount)
    {
        var web3 = new Web3(_orchestrator, _rpcUrl);
        var txnDispatcher = web3.Eth.GetContractTransactionHandler<TransferMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truthserumAddress,
            new()
            {
                To = address,
                Amount = amount
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task DepositFunds(string privateKey, BigInteger amount)
    {
        var player = new Account(privateKey);
        var web3 = new Web3(player, _rpcUrl);

        var approveTxnDispatcher = web3.Eth.GetContractTransactionHandler<ApproveMessage>();
        var approveTxnReceipt = await approveTxnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truthserumAddress,
            new()
            {
                Spender = _truQuestAddress,
                Amount = amount
            }
        );

        await _blockchainManipulator.Mine(1);

        var depositTxnDispatcher = web3.Eth.GetContractTransactionHandler<DepositMessage>();
        var depositTxnReceipt = await depositTxnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truQuestAddress,
            new()
            {
                Amount = amount
            }
        );

        await _blockchainManipulator.Mine(1);

        var balance = await web3.Eth.GetContractQueryHandler<GetAvailableFundsMessage>()
            .QueryAsync<BigInteger>(
                _truQuestAddress,
                new()
                {
                    User = player.Address
                }
            );

        _logger.LogInformation("User {Address} balance: {Balance}", player.Address, balance);
    }

    public async Task FundThing(ThingVm thing, string signature)
    {
        signature = signature.Substring(2);
        var r = signature.Substring(0, 64).HexToByteArray();
        var s = signature.Substring(64, 64).HexToByteArray();
        var v = (byte)signature.Substring(128, 2).HexToBigInteger(true);

        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<FundThingMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truQuestAddress,
            new()
            {
                Thing = new ThingTd
                {
                    Id = thing.Id
                },
                V = v,
                R = r,
                S = s
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task PreJoinLotteryAs(string privateKey, string thingId, byte[] dataHash)
    {
        var lotteryPlayer = new Account(privateKey);
        var web3 = new Web3(lotteryPlayer, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<PreJoinLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _verifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                DataHash = dataHash
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinLotteryAs(string privateKey, string thingId, byte[] data)
    {
        var lotteryPlayer = new Account(privateKey);
        var web3 = new Web3(lotteryPlayer, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<JoinLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _verifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                Data = data
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public Task<int> GetLotteryDurationBlocks()
    {
        return _web3.Eth.GetContractQueryHandler<GetLotteryDurationBlocksMessage>()
            .QueryAsync<int>(_verifierLotteryAddress, new());
    }

    public Task<long> GetLotteryInitBlockNumber(string thingId)
    {
        return _web3.Eth.GetContractQueryHandler<GetLotteryInitBlockNumberMessage>()
            .QueryAsync<long>(_verifierLotteryAddress, new()
            {
                ThingId = thingId
            });
    }
}