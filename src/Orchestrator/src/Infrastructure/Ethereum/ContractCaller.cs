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
    private readonly string _simpleAccountFactoryAddress;
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
        _simpleAccountFactoryAddress = configuration[$"Ethereum:Contracts:{network}:SimpleAccountFactory:Address"]!;
        _thingSubmissionVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
        _thingAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!;
        _assessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:AssessmentPoll:Address"]!;
    }

    public Task<string> GetWalletAddressFor(string ownerAddress) => _web3.Eth
        .GetContractQueryHandler<GetAddressMessage>()
        .QueryAsync<string>(
            _simpleAccountFactoryAddress,
            new()
            {
                Owner = ownerAddress,
                Salt = 0
            }
        );

    public Task<byte[]> ComputeHashForThingSubmissionVerifierLottery(byte[] data)
    {
        return _web3.Eth.GetContractQueryHandler<ComputeHashMessage>().QueryAsync<byte[]>(
            _thingSubmissionVerifierLotteryAddress, new() { Data = data }
        );
    }

    public async Task<long> InitThingSubmissionVerifierLottery(byte[] thingId, byte[] dataHash, byte[] userXorDataHash)
    {
        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<InitThingSubmissionVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingSubmissionVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                DataHash = dataHash,
                UserXorDataHash = userXorDataHash
            }
        );

        _logger.LogInformation("=============== InitThingSubmissionVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        // @@NOTE: Doing it this way instead of returning block number from the receipt, because
        // even when we are on L2, lottery init block is /L1/ block number.
        return await GetThingSubmissionVerifierLotteryInitBlock(thingId);
    }

    public async Task<long> GetThingSubmissionVerifierLotteryInitBlock(byte[] thingId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetThingSubmissionVerifierLotteryInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _thingSubmissionVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId
                }
            );

        return (long)block;
    }

    public Task<bool> CheckThingSubmissionVerifierLotteryExpired(byte[] thingId) => _web3.Eth
        .GetContractQueryHandler<CheckThingSubmissionVerifierLotteryExpiredMessage>()
        .QueryAsync<bool>(
            _thingSubmissionVerifierLotteryAddress,
            new() { ThingId = thingId }
        );

    public Task<BigInteger> GetThingSubmissionVerifierLotteryMaxNonce() => _web3.Eth
        .GetContractQueryHandler<GetMaxNonceMessage>()
        .QueryAsync<BigInteger>(_thingSubmissionVerifierLotteryAddress, new());

    public async Task CloseThingSubmissionVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, byte[] userXorData, byte[] hashOfL1EndBlock, List<ulong> winnerIndices
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
                    UserXorData = userXorData,
                    HashOfL1EndBlock = hashOfL1EndBlock,
                    WinnerIndices = winnerIndices
                }
            );

        _logger.LogInformation("=============== CloseThingSubmissionVerifierLotteryWithSuccess: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task CloseThingSubmissionVerifierLotteryInFailure(byte[] thingId, int joinedNumVerifiers)
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<CloseThingSubmissionVerifierLotteryInFailureMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingSubmissionVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId,
                    JoinedNumVerifiers = joinedNumVerifiers
                }
            );

        _logger.LogInformation("=============== CloseThingSubmissionVerifierLotteryInFailure: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task<long> GetThingAcceptancePollInitBlock(byte[] thingId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetThingAcceptancePollInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _acceptancePollAddress,
                new() { ThingId = thingId }
            );

        return (long)block;
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

    public async Task FinalizeAcceptancePollForThingAsUnsettledDueToMajorityThresholdNotReached(
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
                    Decision = Decision.UnsettledDueToMajorityThresholdNotReached,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAcceptancePollForThingAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAcceptancePollForThingAsAccepted(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
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
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAcceptancePollForThingAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAcceptancePollForThingAsSoftDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAcceptancePollForThingAsSoftDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _acceptancePollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAcceptancePollForThingAsSoftDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAcceptancePollForThingAsHardDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAcceptancePollForThingAsHardDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _acceptancePollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAcceptancePollForThingAsHardDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
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

    public async Task<long> InitThingAssessmentVerifierLottery(
        byte[] thingId, byte[] proposalId, byte[] dataHash, byte[] userXorDataHash
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<InitThingAssessmentVerifierLotteryMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    DataHash = dataHash,
                    UserXorDataHash = userXorDataHash
                }
            );

        _logger.LogInformation("=============== InitThingAssessmentVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        return await GetThingAssessmentVerifierLotteryInitBlock(thingId, proposalId);
    }

    public async Task<long> GetThingAssessmentVerifierLotteryInitBlock(byte[] thingId, byte[] proposalId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetThingAssessmentVerifierLotteryInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray()
                }
            );

        return (long)block;
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

    public Task<bool> CheckThingAssessmentVerifierLotteryExpired(
        byte[] thingId, byte[] proposalId
    ) => _web3.Eth
        .GetContractQueryHandler<CheckThingAssessmentVerifierLotteryExpiredMessage>()
        .QueryAsync<bool>(
            _thingAssessmentVerifierLotteryAddress,
            new() { ThingProposalId = thingId.Concat(proposalId).ToArray() }
        );

    public Task<BigInteger> GetThingAssessmentVerifierLotteryMaxNonce() => _web3.Eth
        .GetContractQueryHandler<GetMaxNonceMessage>()
        .QueryAsync<BigInteger>(_thingAssessmentVerifierLotteryAddress, new());

    public async Task CloseThingAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] proposalId, byte[] data, byte[] userXorData,
        byte[] hashOfL1EndBlock, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
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
                    UserXorData = userXorData,
                    HashOfL1EndBlock = hashOfL1EndBlock,
                    WinnerClaimantIndices = winnerClaimantIndices,
                    WinnerIndices = winnerIndices
                }
            );

        _logger.LogInformation("=============== CloseThingAssessmentVerifierLotteryWithSuccess: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task CloseThingAssessmentVerifierLotteryInFailure(byte[] thingId, byte[] proposalId, int joinedNumVerifiers)
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<CloseThingAssessmentVerifierLotteryInFailureMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    JoinedNumVerifiers = joinedNumVerifiers
                }
            );

        _logger.LogInformation("=============== CloseThingAssessmentVerifierLotteryInFailure: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task<IEnumerable<string>> GetVerifiersForProposal(byte[] thingId, byte[] proposalId)
    {
        var verifiers = await _web3.Eth
            .GetContractQueryHandler<GetVerifiersForProposalMessage>()
            .QueryAsync<List<string>>(_assessmentPollAddress, new()
            {
                ThingProposalId = thingId.Concat(proposalId).ToArray()
            });

        return verifiers;
    }

    public async Task<long> GetThingAssessmentPollInitBlock(byte[] thingId, byte[] proposalId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetThingAssessmentPollInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _assessmentPollAddress,
                new() { ThingProposalId = thingId.Concat(proposalId).ToArray() }
            );

        return (long)block;
    }

    public async Task FinalizeAssessmentPollForProposalAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAssessmentPollForProposalAsUnsettledMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _assessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    Decision = Decision.UnsettledDueToInsufficientVotingVolume,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAssessmentPollForProposalAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAssessmentPollForProposalAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAssessmentPollForProposalAsUnsettledMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _assessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    Decision = Decision.UnsettledDueToMajorityThresholdNotReached,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAssessmentPollForProposalAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAssessmentPollForProposalAsAccepted(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAssessmentPollForProposalAsAcceptedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _assessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAssessmentPollForProposalAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAssessmentPollForProposalAsSoftDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAssessmentPollForProposalAsSoftDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _assessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAssessmentPollForProposalAsSoftDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeAssessmentPollForProposalAsHardDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<FinalizeAssessmentPollForProposalAsHardDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _assessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            );

        _logger.LogInformation("=============== FinalizeAssessmentPollForProposalAsHardDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }
}
