using System.Numerics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Nethereum.Web3;

using Application.Common.Interfaces;

using Infrastructure.Ethereum.Messages;

namespace Infrastructure.Ethereum;

internal class ContractCaller : IContractCaller
{
    private readonly ILogger<ContractCaller> _logger;
    private readonly AccountProvider _accountProvider;
    private readonly Web3 _web3;
    private readonly string _thingSubmissionVerifierLotteryAddress;
    private readonly string _acceptancePollAddress;
    private readonly string _thingAssessmentVerifierLotteryAddress;
    private readonly string _assessmentPollAddress;

    public ContractCaller(
        ILogger<ContractCaller> logger,
        IConfiguration configuration,
        AccountProvider accountProvider
    )
    {
        _logger = logger;
        _accountProvider = accountProvider;

        var network = configuration["Ethereum:Network"]!;
        var orchestrator = _accountProvider.GetAccount("Orchestrator");
        _web3 = new Web3(orchestrator, configuration[$"Ethereum:Networks:{network}:URL"]);
        _thingSubmissionVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
        _thingAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!;
        _assessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:AssessmentPoll:Address"]!;
    }

    public Task<byte[]> ComputeHashForThingSubmissionVerifierLottery(byte[] data)
    {
        return _web3.Eth.GetContractQueryHandler<ComputeHashMessage>().QueryAsync<byte[]>(
            _thingSubmissionVerifierLotteryAddress, new() { Data = data }
        );
    }

    public async Task<long> InitThingSubmissionVerifierLottery(byte[] thingId, byte[] dataHash)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<InitVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingSubmissionVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                DataHash = dataHash
            }
        );

        _logger.LogInformation("=============== InitThingSubmissionVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        return (long)txnReceipt.BlockNumber.Value;
    }

    public Task<BigInteger> ComputeNonceForThingSubmissionVerifierLottery(byte[] thingId, byte[] data)
    {
        return _web3.Eth.GetContractQueryHandler<ComputeNonceMessage>().QueryAsync<BigInteger>(
            _thingSubmissionVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                Data = data
            }
        );
    }

    public async Task CloseThingSubmissionVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, List<ulong> winnerIndices
    )
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<CloseThingSubmissionVerifierLotteryWithSuccessMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingSubmissionVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                Data = data,
                WinnerIndices = winnerIndices
            }
        );

        _logger.LogInformation("=============== CloseThingSubmissionVerifierLotteryWithSuccess: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAcceptancePollForThingAsAccepted(
        byte[] thingId, string voteAggIpfsCid,
        List<string> verifiersToReward, List<string> verifiersToSlash
    )
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<FinalizeAcceptancePollForThingAsAcceptedMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _acceptancePollAddress,
            new()
            {
                ThingId = thingId,
                VoteAggIpfsCid = voteAggIpfsCid,
                VerifiersToReward = verifiersToReward,
                VerifiersToSlash = verifiersToSlash
            }
        );

        _logger.LogInformation("=============== FinalizeAcceptancePollForThingAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task<long> InitThingAssessmentVerifierLottery(byte[] thingId, byte[] dataHash)
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

    public Task<BigInteger> ComputeNonceForThingAssessmentVerifierLottery(byte[] thingId, byte[] data)
    {
        return _web3.Eth.GetContractQueryHandler<ComputeNonceMessage>().QueryAsync<BigInteger>(
            _thingAssessmentVerifierLotteryAddress,
            new()
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
        byte[] thingId, byte[] data, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
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

    public async Task FinalizeAssessmentPollForSettlementProposalAsAccepted(
        byte[] thingId, byte[] settlementProposalId, string voteAggIpfsCid,
        List<string> verifiersToReward, List<string> verifiersToSlash
    )
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<FinalizeAssessmentPollForSettlementProposalAsAcceptedMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _assessmentPollAddress,
            new()
            {
                CombinedId = thingId.Concat(settlementProposalId).ToArray(),
                VoteAggIpfsCid = voteAggIpfsCid,
                VerifiersToReward = verifiersToReward,
                VerifiersToSlash = verifiersToSlash
            }
        );

        _logger.LogInformation("=============== FinalizeAssessmentPollForSettlementProposalAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }
}