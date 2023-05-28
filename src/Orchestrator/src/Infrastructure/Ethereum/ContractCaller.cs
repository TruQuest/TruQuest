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
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<InitThingSubmissionVerifierLotteryMessage>();
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

    public Task<BigInteger> ComputeNonceForThingSubmissionVerifierLottery(
        byte[] thingId, string accountName, byte[] data
    )
    {
        return _web3.Eth
            .GetContractQueryHandler<ComputeNonceForThingSubmissionVerifierLotteryMessage>()
            .QueryAsync<BigInteger>(
                _thingSubmissionVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId,
                    User = _accountProvider.GetAccount(accountName).Address,
                    Data = data
                }
            );
    }

    public async Task CloseThingSubmissionVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, List<ulong> winnerIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<CloseThingSubmissionVerifierLotteryWithSuccessMessage>()
            .SendRequestAndWaitForReceiptAsync(
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

    public async Task FinalizeAcceptancePollForThingAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAcceptancePollForThingAsUnsettledMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _acceptancePollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    Decision = Decision.UnsettledDueToInsufficientVotingVolume,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAcceptancePollForThingAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAcceptancePollForThingAsAccepted(
        byte[] thingId, string voteAggIpfsCid,
        List<string> verifiersToReward, List<string> verifiersToSlash
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAcceptancePollForThingAsAcceptedMessage>()
            .SendRequestAndWaitForReceiptAsync(
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

    public async Task<IEnumerable<string>> GetVerifiersForThing(byte[] thingId)
    {
        var verifiers = await _web3.Eth
            .GetContractQueryHandler<GetVerifiersForThingMessage>()
            .QueryAsync<List<string>>(_acceptancePollAddress, new()
            {
                ThingId = thingId
            });

        return verifiers;
    }

    public async Task<long> InitThingAssessmentVerifierLottery(byte[] thingId, byte[] proposalId, byte[] dataHash)
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<InitThingAssessmentVerifierLotteryMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    DataHash = dataHash
                }
            );

        _logger.LogInformation("=============== InitThingAssessmentVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        return (long)txnReceipt.BlockNumber.Value;
    }

    public Task<BigInteger> ComputeNonceForThingAssessmentVerifierLottery(
        byte[] thingId, byte[] proposalId, string accountName, byte[] data
    )
    {
        return _web3.Eth
            .GetContractQueryHandler<ComputeNonceForThingAssessmentVerifierLotteryMessage>()
            .QueryAsync<BigInteger>(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    User = _accountProvider.GetAccount(accountName).Address,
                    Data = data
                }
            );
    }

    public Task<byte[]> ComputeHashForThingAssessmentVerifierLottery(byte[] data)
    {
        return _web3.Eth
            .GetContractQueryHandler<ComputeHashMessage>()
            .QueryAsync<byte[]>(
                _thingAssessmentVerifierLotteryAddress,
                new() { Data = data }
            );
    }

    public async Task CloseThingAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] proposalId, byte[] data, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<CloseThingAssessmentVerifierLotteryWithSuccessMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
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
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAssessmentPollForSettlementProposalAsAcceptedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _assessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(settlementProposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToReward = verifiersToReward,
                    VerifiersToSlash = verifiersToSlash
                }
            );

        _logger.LogInformation("=============== FinalizeAssessmentPollForSettlementProposalAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }
}