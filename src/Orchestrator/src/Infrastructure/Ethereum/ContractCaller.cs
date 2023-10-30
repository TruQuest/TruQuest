using System.Numerics;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;

using Application;
using Application.Common.Interfaces;
using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;

using Infrastructure.Ethereum.Messages;
using Infrastructure.Ethereum.TypedData;

namespace Infrastructure.Ethereum;

internal class ContractCaller : IContractCaller
{
    private readonly ILogger<ContractCaller> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly AccountProvider _accountProvider;
    private readonly Web3 _web3;
    private readonly string _entryPointAddress;
    private readonly string _simpleAccountFactoryAddress;
    private readonly string _thingValidationVerifierLotteryAddress;
    private readonly string _thingValidationPollAddress;
    private readonly string _settlementProposalAssessmentVerifierLotteryAddress;
    private readonly string _settlementProposalAssessmentPollAddress;

    private readonly ChannelReader<(
        TaskCompletionSource<TransactionReceipt> Tcs,
        Func<Task<TransactionReceipt>> Task,
        string Traceparent
    )> _stream;

    private readonly ChannelWriter<(
        TaskCompletionSource<TransactionReceipt> Tcs,
        Func<Task<TransactionReceipt>> Task,
        string Traceparent
    )> _sink;

    public ContractCaller(
        ILogger<ContractCaller> logger,
        IMemoryCache memoryCache,
        IConfiguration configuration,
        AccountProvider accountProvider,
        IHostApplicationLifetime appLifetime
    )
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _accountProvider = accountProvider;

        var network = configuration["Ethereum:Network"]!;
        var orchestrator = _accountProvider.GetAccount("Orchestrator");
        _web3 = new Web3(orchestrator, configuration[$"Ethereum:Networks:{network}:URL"]);
        _entryPointAddress = configuration[$"Ethereum:Contracts:{network}:EntryPoint:Address"]!;
        _simpleAccountFactoryAddress = configuration[$"Ethereum:Contracts:{network}:SimpleAccountFactory:Address"]!;
        _thingValidationVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationVerifierLottery:Address"]!;
        _thingValidationPollAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationPoll:Address"]!;
        _settlementProposalAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentVerifierLottery:Address"]!;
        _settlementProposalAssessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentPoll:Address"]!;

        var channel = Channel.CreateUnbounded<(
            TaskCompletionSource<TransactionReceipt> Tcs,
            Func<Task<TransactionReceipt>> Task,
            string Traceparent
        )>(new UnboundedChannelOptions { SingleReader = true });

        _stream = channel.Reader;
        _sink = channel.Writer;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(appLifetime.ApplicationStopping);
        _monitorMessages(cts.Token);
    }

    private async void _monitorMessages(CancellationToken ct)
    {
        // @@TODO!!: Try-catch!
        await foreach (var item in _stream.ReadAllAsync(ct))
        {
            var txnReceipt = await item.Task();
            _memoryCache.Set(txnReceipt.TransactionHash, item.Traceparent);
            item.Tcs.SetResult(txnReceipt);
        }
    }

    private async Task<TransactionReceipt> _sendTxn(Func<Task<TransactionReceipt>> task)
    {
        var tcs = new TaskCompletionSource<TransactionReceipt>();
        await _sink.WriteAsync((tcs, task, Telemetry.CurrentActivity!.GetTraceparent()));
        return await tcs.Task;
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

    public Task<BigInteger> GetWalletNonce(string walletAddress) => _web3.Eth
        .GetContractQueryHandler<GetNonceMessage>()
        .QueryAsync<BigInteger>(
            _entryPointAddress,
            new()
            {
                Sender = walletAddress,
                Key = 0
            }
        );

    public Task<byte[]> GetUserOperationHash(UserOperation userOp) => _web3.Eth
        .GetContractQueryHandler<GetUserOpHashMessage>()
        .QueryAsync<byte[]>(
            _entryPointAddress,
            new()
            {
                UserOp = new UserOperationTd
                {
                    Sender = userOp.Sender,
                    Nonce = userOp.Nonce,
                    InitCode = userOp.InitCode.HexToByteArray(),
                    CallData = userOp.CallData.HexToByteArray(),
                    CallGasLimit = userOp.CallGasLimit,
                    VerificationGasLimit = userOp.VerificationGasLimit,
                    PreVerificationGas = userOp.PreVerificationGas,
                    MaxFeePerGas = userOp.MaxFeePerGas,
                    MaxPriorityFeePerGas = userOp.MaxPriorityFeePerGas,
                    PaymasterAndData = userOp.PaymasterAndData.HexToByteArray(),
                    Signature = userOp.Signature.HexToByteArray()
                }
            }
        );

    public Task<int> GetThingValidationVerifierLotteryNumVerifiers() => _web3.Eth
        .GetContractQueryHandler<GetNumVerifiersMessage>()
        .QueryAsync<int>(_thingValidationVerifierLotteryAddress, new());

    public Task<int> GetThingValidationVerifierLotteryDurationBlocks() => _web3.Eth
        .GetContractQueryHandler<GetDurationBlocksMessage>()
        .QueryAsync<int>(_thingValidationVerifierLotteryAddress, new());

    public async Task<IEnumerable<string>> GetThingValidationVerifierLotteryParticipants(byte[] thingId) => await _web3.Eth
        .GetContractQueryHandler<GetThingValidationVerifierLotteryParticipantsMessage>()
        .QueryAsync<List<string>>(
            _thingValidationVerifierLotteryAddress,
            new()
            {
                ThingId = thingId
            }
        );

    public Task<byte[]> ComputeHashForThingValidationVerifierLottery(byte[] data)
    {
        return _web3.Eth.GetContractQueryHandler<ComputeHashMessage>().QueryAsync<byte[]>(
            _thingValidationVerifierLotteryAddress, new() { Data = data }
        );
    }

    public async Task<long> InitThingValidationVerifierLottery(byte[] thingId, byte[] dataHash, byte[] userXorDataHash)
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<InitThingValidationVerifierLotteryMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId,
                    DataHash = dataHash,
                    UserXorDataHash = userXorDataHash
                }
            )
        );

        _logger.LogInformation("=============== InitThingValidationVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        // @@NOTE: Doing it this way instead of returning block number from the receipt, because
        // even when we are on L2, lottery init block is /L1/ block number.
        return await GetThingValidationVerifierLotteryInitBlock(thingId);
    }

    public async Task<long> GetThingValidationVerifierLotteryInitBlock(byte[] thingId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetThingValidationVerifierLotteryInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _thingValidationVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId
                }
            );

        return (long)block;
    }

    public Task<bool> CheckThingValidationVerifierLotteryExpired(byte[] thingId) => _web3.Eth
        .GetContractQueryHandler<CheckThingValidationVerifierLotteryExpiredMessage>()
        .QueryAsync<bool>(
            _thingValidationVerifierLotteryAddress,
            new() { ThingId = thingId }
        );

    public Task<BigInteger> GetThingValidationVerifierLotteryMaxNonce() => _web3.Eth
        .GetContractQueryHandler<GetMaxNonceMessage>()
        .QueryAsync<BigInteger>(_thingValidationVerifierLotteryAddress, new());

    public async Task CloseThingValidationVerifierLotteryWithSuccess(
        byte[] thingId, byte[] data, byte[] userXorData, byte[] hashOfL1EndBlock, List<ulong> winnerIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<CloseThingValidationVerifierLotteryWithSuccessMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId,
                    Data = data,
                    UserXorData = userXorData,
                    HashOfL1EndBlock = hashOfL1EndBlock,
                    WinnerIndices = winnerIndices
                }
            )
        );

        _logger.LogInformation("=============== CloseThingValidationVerifierLotteryWithSuccess: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task CloseThingValidationVerifierLotteryInFailure(byte[] thingId, int joinedNumVerifiers)
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<CloseThingValidationVerifierLotteryInFailureMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId,
                    JoinedNumVerifiers = joinedNumVerifiers
                }
            )
        );

        _logger.LogInformation("=============== CloseThingValidationVerifierLotteryInFailure: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public Task<int> GetThingValidationPollVotingVolumeThresholdPercent() => _web3.Eth
        .GetContractQueryHandler<GetPollVotingVolumeThresholdPercentMessage>()
        .QueryAsync<int>(_thingValidationPollAddress, new());

    public Task<int> GetThingValidationPollMajorityThresholdPercent() => _web3.Eth
        .GetContractQueryHandler<GetPollMajorityThresholdPercentMessage>()
        .QueryAsync<int>(_thingValidationPollAddress, new());

    public Task<int> GetThingValidationPollDurationBlocks() => _web3.Eth
        .GetContractQueryHandler<GetDurationBlocksMessage>()
        .QueryAsync<int>(_thingValidationPollAddress, new());

    public async Task<long> GetThingValidationPollInitBlock(byte[] thingId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetThingValidationPollInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _thingValidationPollAddress,
                new() { ThingId = thingId }
            );

        return (long)block;
    }

    public async Task FinalizeThingValidationPollAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeThingValidationPollAsUnsettledMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationPollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    Decision = Decision.UnsettledDueToInsufficientVotingVolume,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeThingValidationPollAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeThingValidationPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeThingValidationPollAsUnsettledMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationPollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    Decision = Decision.UnsettledDueToMajorityThresholdNotReached,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeThingValidationPollAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeThingValidationPollAsAccepted(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeThingValidationPollAsAcceptedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationPollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeThingValidationPollAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeThingValidationPollAsSoftDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeThingValidationPollAsSoftDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationPollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeThingValidationPollAsSoftDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeThingValidationPollAsHardDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeThingValidationPollAsHardDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _thingValidationPollAddress,
                new()
                {
                    ThingId = thingId,
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeThingValidationPollAsHardDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task<IEnumerable<string>> GetVerifiersForThing(byte[] thingId) => await _web3.Eth
        .GetContractQueryHandler<GetThingVerifiersMessage>()
        .QueryAsync<List<string>>(_thingValidationPollAddress, new()
        {
            ThingId = thingId
        });

    public Task<int> GetSettlementProposalAssessmentVerifierLotteryNumVerifiers() => _web3.Eth
        .GetContractQueryHandler<GetNumVerifiersMessage>()
        .QueryAsync<int>(_settlementProposalAssessmentVerifierLotteryAddress, new());

    public Task<int> GetSettlementProposalAssessmentVerifierLotteryDurationBlocks() => _web3.Eth
        .GetContractQueryHandler<GetDurationBlocksMessage>()
        .QueryAsync<int>(_settlementProposalAssessmentVerifierLotteryAddress, new());

    public async Task<IEnumerable<string>> GetSettlementProposalAssessmentVerifierLotterySpotClaimants(byte[] thingId, byte[] proposalId) =>
        await _web3.Eth
            .GetContractQueryHandler<GetSettlementProposalAssessmentVerifierLotterySpotClaimantsMessage>()
            .QueryAsync<List<string>>(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray()
                }
            );

    public async Task<IEnumerable<string>> GetSettlementProposalAssessmentVerifierLotteryParticipants(byte[] thingId, byte[] proposalId) =>
        await _web3.Eth
            .GetContractQueryHandler<GetSettlementProposalAssessmentVerifierLotteryParticipantsMessage>()
            .QueryAsync<List<string>>(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray()
                }
            );

    public async Task<long> InitSettlementProposalAssessmentVerifierLottery(
        byte[] thingId, byte[] proposalId, byte[] dataHash, byte[] userXorDataHash
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<InitSettlementProposalAssessmentVerifierLotteryMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    DataHash = dataHash,
                    UserXorDataHash = userXorDataHash
                }
            )
        );

        _logger.LogInformation("=============== InitSettlementProposalAssessmentVerifierLottery: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);

        return await GetSettlementProposalAssessmentVerifierLotteryInitBlock(thingId, proposalId);
    }

    public async Task<long> GetSettlementProposalAssessmentVerifierLotteryInitBlock(byte[] thingId, byte[] proposalId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetSettlementProposalAssessmentVerifierLotteryInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray()
                }
            );

        return (long)block;
    }

    public Task<byte[]> ComputeHashForSettlementProposalAssessmentVerifierLottery(byte[] data)
    {
        return _web3.Eth
            .GetContractQueryHandler<ComputeHashMessage>()
            .QueryAsync<byte[]>(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new() { Data = data }
            );
    }

    public Task<bool> CheckSettlementProposalAssessmentVerifierLotteryExpired(
        byte[] thingId, byte[] proposalId
    ) => _web3.Eth
        .GetContractQueryHandler<CheckSettlementProposalAssessmentVerifierLotteryExpiredMessage>()
        .QueryAsync<bool>(
            _settlementProposalAssessmentVerifierLotteryAddress,
            new() { ThingProposalId = thingId.Concat(proposalId).ToArray() }
        );

    public Task<BigInteger> GetSettlementProposalAssessmentVerifierLotteryMaxNonce() => _web3.Eth
        .GetContractQueryHandler<GetMaxNonceMessage>()
        .QueryAsync<BigInteger>(_settlementProposalAssessmentVerifierLotteryAddress, new());

    public async Task CloseSettlementProposalAssessmentVerifierLotteryWithSuccess(
        byte[] thingId, byte[] proposalId, byte[] data, byte[] userXorData,
        byte[] hashOfL1EndBlock, List<ulong> winnerClaimantIndices, List<ulong> winnerIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<CloseSettlementProposalAssessmentVerifierLotteryWithSuccessMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    Data = data,
                    UserXorData = userXorData,
                    HashOfL1EndBlock = hashOfL1EndBlock,
                    WinnerClaimantIndices = winnerClaimantIndices,
                    WinnerIndices = winnerIndices
                }
            )
        );

        _logger.LogInformation("=============== CloseSettlementProposalAssessmentVerifierLotteryWithSuccess: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task CloseSettlementProposalAssessmentVerifierLotteryInFailure(byte[] thingId, byte[] proposalId, int joinedNumVerifiers)
    {
        var txnReceipt = await _web3.Eth
            .GetContractTransactionHandler<CloseSettlementProposalAssessmentVerifierLotteryInFailureMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    JoinedNumVerifiers = joinedNumVerifiers
                }
            );

        _logger.LogInformation("=============== CloseSettlementProposalAssessmentVerifierLotteryInFailure: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task<IEnumerable<string>> GetVerifiersForSettlementProposal(byte[] thingId, byte[] proposalId)
    {
        var verifiers = await _web3.Eth
            .GetContractQueryHandler<GetSettlementProposalVerifiersMessage>()
            .QueryAsync<List<string>>(_settlementProposalAssessmentPollAddress, new()
            {
                ThingProposalId = thingId.Concat(proposalId).ToArray()
            });

        return verifiers;
    }

    public Task<int> GetSettlementProposalAssessmentPollVotingVolumeThresholdPercent() => _web3.Eth
        .GetContractQueryHandler<GetPollVotingVolumeThresholdPercentMessage>()
        .QueryAsync<int>(_settlementProposalAssessmentPollAddress, new());

    public Task<int> GetSettlementProposalAssessmentPollMajorityThresholdPercent() => _web3.Eth
        .GetContractQueryHandler<GetPollMajorityThresholdPercentMessage>()
        .QueryAsync<int>(_settlementProposalAssessmentPollAddress, new());

    public Task<int> GetSettlementProposalAssessmentPollDurationBlocks() => _web3.Eth
        .GetContractQueryHandler<GetDurationBlocksMessage>()
        .QueryAsync<int>(_settlementProposalAssessmentPollAddress, new());

    public async Task<long> GetSettlementProposalAssessmentPollInitBlock(byte[] thingId, byte[] proposalId)
    {
        var block = await _web3.Eth
            .GetContractQueryHandler<GetSettlementProposalAssessmentPollInitBlockMessage>()
            .QueryAsync<BigInteger>(
                _settlementProposalAssessmentPollAddress,
                new() { ThingProposalId = thingId.Concat(proposalId).ToArray() }
            );

        return (long)block;
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsUnsettledDueToInsufficientVotingVolume(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsUnsettledMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    Decision = Decision.UnsettledDueToInsufficientVotingVolume,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeSettlementProposalAssessmentPollAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsUnsettledMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    Decision = Decision.UnsettledDueToMajorityThresholdNotReached,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeSettlementProposalAssessmentPollAsUnsettled: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsAccepted(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsAcceptedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeSettlementProposalAssessmentPollAsAccepted: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsSoftDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsSoftDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeSettlementProposalAssessmentPollAsSoftDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsHardDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        var txnReceipt = await _sendTxn(() => _web3.Eth
            .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsHardDeclinedMessage>()
            .SendRequestAndWaitForReceiptAsync(
                _settlementProposalAssessmentPollAddress,
                new()
                {
                    ThingProposalId = thingId.Concat(proposalId).ToArray(),
                    VoteAggIpfsCid = voteAggIpfsCid,
                    VerifiersToSlashIndices = verifiersToSlashIndices
                }
            )
        );

        _logger.LogInformation("=============== FinalizeSettlementProposalAssessmentPollAsHardDeclined: Txn hash {TxnHash} ===============", txnReceipt.TransactionHash);
    }
}
