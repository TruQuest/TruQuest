using System.Numerics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Nethereum.Web3;
using Nethereum.Web3.Accounts;

using Application.Common.Interfaces;

using Infrastructure.Ethereum.Messages;

namespace Infrastructure.Ethereum;

internal class ContractCaller : IContractCaller
{
    private readonly ILogger<ContractCaller> _logger;
    private readonly Web3 _web3;
    private readonly string _verifierLotteryAddress;
    private readonly string _acceptancePollAddress;
    private readonly string _thingAssessmentVerifierLotteryAddress;

    public ContractCaller(ILogger<ContractCaller> logger, IConfiguration configuration)
    {
        _logger = logger;

        var network = configuration["Ethereum:Network"]!;
        var orchestrator = new Account(configuration[$"Ethereum:Accounts:{network}:Orchestrator:PrivateKey"]);
        _web3 = new Web3(orchestrator, configuration[$"Ethereum:Networks:{network}:URL"]);
        _verifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
        _thingAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!;
    }

    public Task<byte[]> ComputeHashForThingSubmissionVerifierLottery(byte[] data)
    {
        var queryDispatcher = _web3.Eth.GetContractQueryHandler<ComputeHashMessage>();
        return queryDispatcher.QueryAsync<byte[]>(_verifierLotteryAddress, new ComputeHashMessage { Data = data });
    }

    public async Task<long> InitVerifierLottery(string thingId, byte[] dataHash)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<InitVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _verifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                DataHash = dataHash
            }
        );

        _logger.LogInformation("=============== InitVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        return (long)txnReceipt.BlockNumber.Value;
    }

    public Task<BigInteger> ComputeNonce(string thingId, byte[] data)
    {
        var queryDispatcher = _web3.Eth.GetContractQueryHandler<ComputeNonceMessage>();
        return queryDispatcher.QueryAsync<BigInteger>(
            _verifierLotteryAddress,
            new ComputeNonceMessage
            {
                ThingId = thingId,
                Data = data
            }
        );
    }

    public async Task CloseVerifierLotteryWithSuccess(string thingId, byte[] data, List<ulong> winnerIndices)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<CloseVerifierLotteryWithSuccessMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _verifierLotteryAddress,
            new CloseVerifierLotteryWithSuccessMessage
            {
                ThingId = thingId,
                Data = data,
                WinnerIndices = winnerIndices
            }
        );

        _logger.LogInformation("=============== CloseVerifierLotteryWithSuccess: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAcceptancePollForThingAsAccepted(
        string thingId, string voteAggIpfsCid,
        List<string> verifiersToReward, List<string> verifiersToSlash
    )
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<FinalizeAcceptancePollForThingAsAcceptedMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _acceptancePollAddress,
            new FinalizeAcceptancePollForThingAsAcceptedMessage
            {
                ThingId = thingId,
                VoteAggIpfsCid = voteAggIpfsCid,
                VerifiersToReward = verifiersToReward,
                VerifiersToSlash = verifiersToSlash
            }
        );

        _logger.LogInformation("=============== FinalizeAcceptancePollForThingAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task<long> InitThingAssessmentVerifierLottery(string thingId, byte[] dataHash)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<InitVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingAssessmentVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                DataHash = dataHash
            }
        );

        _logger.LogInformation("=============== InitThingAssessmentVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        return (long)txnReceipt.BlockNumber.Value;
    }

    public Task<BigInteger> ComputeNonceForThingAssessmentVerifierLottery(string thingId, byte[] data)
    {
        var queryDispatcher = _web3.Eth.GetContractQueryHandler<ComputeNonceMessage>();
        return queryDispatcher.QueryAsync<BigInteger>(
            _thingAssessmentVerifierLotteryAddress,
            new ComputeNonceMessage
            {
                ThingId = thingId,
                Data = data
            }
        );
    }

    public Task<byte[]> ComputeHashForThingAssessmentVerifierLottery(byte[] data)
    {
        var queryDispatcher = _web3.Eth.GetContractQueryHandler<ComputeHashMessage>();
        return queryDispatcher.QueryAsync<byte[]>(_thingAssessmentVerifierLotteryAddress, new() { Data = data });
    }

    public async Task CloseThingAssessmentVerifierLotteryWithSuccess(
        string thingId, byte[] data, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    )
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<CloseThingAssessmentVerifierLotteryWithSuccessMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingAssessmentVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                Data = data,
                WinnerClaimantIndices = winnerClaimantIndices,
                WinnerIndices = winnerIndices
            }
        );

        _logger.LogInformation("=============== CloseThingAssessmentVerifierLotteryWithSuccess: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }
}