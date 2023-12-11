using System.Numerics;
using System.Threading.Channels;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;

using Domain.Results;
using Application.Common.Monitoring;
using Application.Common.Interfaces;
using Application.Common.Misc;
using Application.Ethereum.Common.Models.IM;
using Application.General.Queries.GetContractsStates.QM;
using static Application.Common.Monitoring.LogMessagePlaceholders;

using Infrastructure.Ethereum.Messages;
using Infrastructure.Ethereum.TypedData;
using Infrastructure.Ethereum.Errors;
using Infrastructure.Ethereum.Messages.Export;

namespace Infrastructure.Ethereum;

internal class ContractCaller : IContractCaller
{
    private readonly ILogger<ContractCaller> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly Web3 _web3;
    private readonly string _entryPointAddress;
    private readonly string _accountFactoryAddress;
    private readonly string _restrictedAccessAddress;
    private readonly string _truquestAddress;
    private readonly string _thingValidationVerifierLotteryAddress;
    private readonly string _thingValidationPollAddress;
    private readonly string _settlementProposalAssessmentVerifierLotteryAddress;
    private readonly string _settlementProposalAssessmentPollAddress;

    private readonly ChannelReader<(
        TaskCompletionSource<Either<BaseError, TransactionReceipt>> Tcs,
        Func<Task<Either<BaseError, TransactionReceipt>>> Task,
        string Traceparent,
        string FunctionName
    )> _stream;

    private readonly ChannelWriter<(
        TaskCompletionSource<Either<BaseError, TransactionReceipt>> Tcs,
        Func<Task<Either<BaseError, TransactionReceipt>>> Task,
        string Traceparent,
        string FunctionName
    )> _sink;

    public ContractCaller(
        ILogger<ContractCaller> logger,
        IMemoryCache memoryCache,
        IConfiguration configuration,
        IAccountProvider accountProvider,
        IHostApplicationLifetime appLifetime
    )
    {
        _logger = logger;
        _memoryCache = memoryCache;

        var network = configuration["Ethereum:Network"]!;
        var orchestrator = accountProvider.GetAccount("Orchestrator");
        _web3 = new Web3(orchestrator, configuration[$"Ethereum:Networks:{network}:URL"]);
        _entryPointAddress = configuration[$"Ethereum:Contracts:{network}:EntryPoint:Address"]!;
        _accountFactoryAddress = configuration[$"Ethereum:Contracts:{network}:AccountFactory:Address"]!;
        _restrictedAccessAddress = configuration[$"Ethereum:Contracts:{network}:RestrictedAccess:Address"]!;
        _truquestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
        _thingValidationVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationVerifierLottery:Address"]!;
        _thingValidationPollAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationPoll:Address"]!;
        _settlementProposalAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentVerifierLottery:Address"]!;
        _settlementProposalAssessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentPoll:Address"]!;

        var channel = Channel.CreateUnbounded<(
            TaskCompletionSource<Either<BaseError, TransactionReceipt>> Tcs,
            Func<Task<Either<BaseError, TransactionReceipt>>> Task,
            string Traceparent,
            string FunctionName
        )>(new UnboundedChannelOptions { SingleReader = true });

        _stream = channel.Reader;
        _sink = channel.Writer;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(appLifetime.ApplicationStopping);
        _monitorMessages(cts.Token);
    }

    private async void _monitorMessages(CancellationToken ct)
    {
        try
        {
            await foreach (var item in _stream.ReadAllAsync(ct))
            {
                // @@TODO!!: Try-catch common errors like insufficient funds, OOG, etc.
                var result = await item.Task();
                if (!result.IsError)
                {
                    _memoryCache.Set(result.Data!.TransactionHash, item.Traceparent);
                    Metrics.FunctionNameToGasUsedHistogram[item.FunctionName].Record((int)result.Data!.GasUsed.Value);
                }
                item.Tcs.SetResult(result);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    private async Task<Either<BaseError, TransactionReceipt>> _sendTxn(
        Func<Task<Either<BaseError, TransactionReceipt>>> task,
        [CallerMemberName] string callerMethodName = ""
    )
    {
        var tcs = new TaskCompletionSource<Either<BaseError, TransactionReceipt>>();
        await _sink.WriteAsync((tcs, task, Telemetry.CurrentActivity!.GetTraceparent(), callerMethodName));
        return await tcs.Task;
    }

    public Task<string> GetWalletAddressFor(string ownerAddress) => _web3.Eth
        .GetContractQueryHandler<GetAddressMessage>()
        .QueryAsync<string>(
            _accountFactoryAddress,
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
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(InitThingValidationVerifierLottery)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<InitThingValidationVerifierLotteryMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _thingValidationVerifierLotteryAddress,
                        new()
                        {
                            ThingId = thingId,
                            DataHash = dataHash,
                            UserXorDataHash = userXorDataHash
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                BaseError error;
                if (ex.IsCustomErrorFor<Errors.ThingValidationVerifierLottery.AlreadyInitializedError>())
                {
                    error = ex.DecodeError<Errors.ThingValidationVerifierLottery.AlreadyInitializedError>();
                }
                else
                {
                    throw new NotImplementedException();
                }

                return error;
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to initialize thing {ThingId} validation verifier lottery: {result.Error}", new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Initialized thing {ThingId} validation verifier lottery.\nTxn hash: {TxnHash}",
                new Guid(thingId), result.Data!.TransactionHash
            );
        }

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
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(CloseThingValidationVerifierLotteryWithSuccess)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
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
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                // @@TODO!!
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to close thing {ThingId} validation verifier lottery with success: {result.Error}", new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Closed thing {ThingId} validation verifier lottery with success.\nTxn hash: {TxnHash}",
                new Guid(thingId), result.Data!.TransactionHash
            );
        }
    }

    public async Task CloseThingValidationVerifierLotteryInFailure(byte[] thingId, int joinedNumVerifiers)
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(CloseThingValidationVerifierLotteryInFailure)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<CloseThingValidationVerifierLotteryInFailureMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _thingValidationVerifierLotteryAddress,
                        new()
                        {
                            ThingId = thingId,
                            JoinedNumVerifiers = joinedNumVerifiers
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to close thing {ThingId} validation verifier lottery in failure: {result.Error}", new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Closed thing {ThingId} validation verifier lottery in failure.\nTxn hash: {TxnHash}",
                new Guid(thingId), result.Data!.TransactionHash
            );
        }
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
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeThingValidationPollAsUnsettledDueToInsufficientVotingVolume)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeThingValidationPollAsUnsettledMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _thingValidationPollAddress,
                        new()
                        {
                            ThingId = thingId,
                            VoteAggIpfsCid = voteAggIpfsCid,
                            Decision = Infrastructure.Ethereum.Messages.Decision.UnsettledDueToInsufficientVotingVolume,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize thing {ThingId} validation poll as unsettled due to insufficient voting volume: {result.Error}",
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized thing {ThingId} validation poll as unsettled due to insufficient voting volume.\nTxn hash: {TxnHash}",
                new Guid(thingId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeThingValidationPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeThingValidationPollAsUnsettledDueToMajorityThresholdNotReached)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeThingValidationPollAsUnsettledMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _thingValidationPollAddress,
                        new()
                        {
                            ThingId = thingId,
                            VoteAggIpfsCid = voteAggIpfsCid,
                            Decision = Infrastructure.Ethereum.Messages.Decision.UnsettledDueToMajorityThresholdNotReached,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize thing {ThingId} validation poll as unsettled due to majority threshold not reached: {result.Error}",
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized thing {ThingId} validation poll as unsettled due to majority threshold not reached.\nTxn hash: {TxnHash}",
                new Guid(thingId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeThingValidationPollAsAccepted(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeThingValidationPollAsAccepted)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeThingValidationPollAsAcceptedMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _thingValidationPollAddress,
                        new()
                        {
                            ThingId = thingId,
                            VoteAggIpfsCid = voteAggIpfsCid,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize thing {ThingId} validation poll as accepted: {result.Error}",
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized thing {ThingId} validation poll as accepted.\nTxn hash: {TxnHash}",
                new Guid(thingId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeThingValidationPollAsSoftDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeThingValidationPollAsSoftDeclined)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeThingValidationPollAsSoftDeclinedMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _thingValidationPollAddress,
                        new()
                        {
                            ThingId = thingId,
                            VoteAggIpfsCid = voteAggIpfsCid,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize thing {ThingId} validation poll as soft declined: {result.Error}",
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized thing {ThingId} validation poll as soft declined.\nTxn hash: {TxnHash}",
                new Guid(thingId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeThingValidationPollAsHardDeclined(
        byte[] thingId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeThingValidationPollAsHardDeclined)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeThingValidationPollAsHardDeclinedMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _thingValidationPollAddress,
                        new()
                        {
                            ThingId = thingId,
                            VoteAggIpfsCid = voteAggIpfsCid,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize thing {ThingId} validation poll as hard declined: {result.Error}",
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized thing {ThingId} validation poll as hard declined.\nTxn hash: {TxnHash}",
                new Guid(thingId),
                result.Data!.TransactionHash
            );
        }
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
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(InitSettlementProposalAssessmentVerifierLottery)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<InitSettlementProposalAssessmentVerifierLotteryMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _settlementProposalAssessmentVerifierLotteryAddress,
                        new()
                        {
                            ThingProposalId = thingId.Concat(proposalId).ToArray(),
                            DataHash = dataHash,
                            UserXorDataHash = userXorDataHash
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to initialize settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment verifier lottery: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Initialized settlement proposal {SettlementProposalId} assessment verifier lottery.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }

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
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(CloseSettlementProposalAssessmentVerifierLotteryWithSuccess)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
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
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to close settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment verifier lottery with success: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Closed settlement proposal {SettlementProposalId} assessment verifier lottery with success.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task CloseSettlementProposalAssessmentVerifierLotteryInFailure(byte[] thingId, byte[] proposalId, int joinedNumVerifiers)
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(CloseSettlementProposalAssessmentVerifierLotteryInFailure)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<CloseSettlementProposalAssessmentVerifierLotteryInFailureMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _settlementProposalAssessmentVerifierLotteryAddress,
                        new()
                        {
                            ThingProposalId = thingId.Concat(proposalId).ToArray(),
                            JoinedNumVerifiers = joinedNumVerifiers
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to close settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment verifier lottery in failure: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Closed settlement proposal {SettlementProposalId} assessment verifier lottery in failure.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }
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
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeSettlementProposalAssessmentPollAsUnsettledDueToInsufficientVotingVolume)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsUnsettledMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _settlementProposalAssessmentPollAddress,
                        new()
                        {
                            ThingProposalId = thingId.Concat(proposalId).ToArray(),
                            VoteAggIpfsCid = voteAggIpfsCid,
                            Decision = Infrastructure.Ethereum.Messages.Decision.UnsettledDueToInsufficientVotingVolume,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment poll as unsettled due to insufficient voting volume: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized settlement proposal {SettlementProposalId} assessment poll as unsettled due to insufficient voting volume.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsUnsettledDueToMajorityThresholdNotReached(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeSettlementProposalAssessmentPollAsUnsettledDueToMajorityThresholdNotReached)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsUnsettledMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _settlementProposalAssessmentPollAddress,
                        new()
                        {
                            ThingProposalId = thingId.Concat(proposalId).ToArray(),
                            VoteAggIpfsCid = voteAggIpfsCid,
                            Decision = Infrastructure.Ethereum.Messages.Decision.UnsettledDueToMajorityThresholdNotReached,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment poll as unsettled due to majority threshold not reached: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized settlement proposal {SettlementProposalId} assessment poll as unsettled due to majority threshold not reached.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsAccepted(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeSettlementProposalAssessmentPollAsAccepted)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsAcceptedMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _settlementProposalAssessmentPollAddress,
                        new()
                        {
                            ThingProposalId = thingId.Concat(proposalId).ToArray(),
                            VoteAggIpfsCid = voteAggIpfsCid,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment poll as accepted: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized settlement proposal {SettlementProposalId} assessment poll as accepted.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsSoftDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeSettlementProposalAssessmentPollAsSoftDeclined)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsSoftDeclinedMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _settlementProposalAssessmentPollAddress,
                        new()
                        {
                            ThingProposalId = thingId.Concat(proposalId).ToArray(),
                            VoteAggIpfsCid = voteAggIpfsCid,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment poll as soft declined: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized settlement proposal {SettlementProposalId} assessment poll as soft declined.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }
    }

    public async Task FinalizeSettlementProposalAssessmentPollAsHardDeclined(
        byte[] thingId, byte[] proposalId, string voteAggIpfsCid, List<ulong> verifiersToSlashIndices
    )
    {
        using var span = Telemetry.StartActivity(
            $"{GetType().FullName}.{nameof(FinalizeSettlementProposalAssessmentPollAsHardDeclined)}",
            kind: ActivityKind.Client
        )!;

        var result = await _sendTxn(async () =>
        {
            try
            {
                return await _web3.Eth
                    .GetContractTransactionHandler<FinalizeSettlementProposalAssessmentPollAsHardDeclinedMessage>()
                    .SendRequestAndWaitForReceiptAsync(
                        _settlementProposalAssessmentPollAddress,
                        new()
                        {
                            ThingProposalId = thingId.Concat(proposalId).ToArray(),
                            VoteAggIpfsCid = voteAggIpfsCid,
                            VerifiersToSlashIndices = verifiersToSlashIndices
                        }
                    );
            }
            catch (SmartContractCustomErrorRevertException ex)
            {
                throw new NotImplementedException();
            }
        });

        if (result.IsError)
        {
            _logger.LogWarning(
                $"Error trying to finalize settlement proposal {SettlementProposalId} (for thing {ThingId}) assessment poll as hard declined: {result.Error}",
                new Guid(proposalId),
                new Guid(thingId)
            );
        }
        else
        {
            _logger.LogInformation(
                $"Finalized settlement proposal {SettlementProposalId} assessment poll as hard declined.\nTxn hash: {TxnHash}",
                new Guid(proposalId),
                result.Data!.TransactionHash
            );
        }
    }

    public Task<List<string>> GetRestrictedAccessWhitelist() => _web3.Eth
        .GetContractQueryHandler<GetWhitelistMessage>()
        .QueryAsync<List<string>>(_restrictedAccessAddress, new());

    public async Task<IEnumerable<UserBalanceQm>> ExportUsersAndBalances()
    {
        var result = await _web3.Eth
            .GetContractQueryHandler<ExportUsersAndBalancesMessage>()
            .QueryAsync<ExportUsersAndBalancesFunctionOutput>(_truquestAddress, new());

        var userBalances = new List<UserBalanceQm>();
        for (int i = 0; i < result.Users.Count; ++i)
        {
            userBalances.Add(new()
            {
                Address = result.Users[i],
                HexBalance = new HexBigInteger(result.Balances[i]).HexValue,
                HexStakedBalance = new HexBigInteger(result.StakedBalances[i]).HexValue
            });
        }

        return userBalances;
    }

    public async Task<IEnumerable<ThingSubmitterQm>> ExportThingSubmitter()
    {
        var result = await _web3.Eth
            .GetContractQueryHandler<ExportThingSubmitterMessage>()
            .QueryAsync<ExportThingSubmitterFunctionOutput>(_truquestAddress, new());

        var thingSubmitters = new List<ThingSubmitterQm>();
        for (int i = 0; i < result.ThingIds.Count; ++i)
        {
            thingSubmitters.Add(new()
            {
                ThingId = new Guid(result.ThingIds[i]),
                Submitter = result.Submitters[i]
            });
        }

        return thingSubmitters;
    }

    public async Task<IEnumerable<SettlementProposalSubmitterQm>> ExportThingIdToSettlementProposal()
    {
        var result = await _web3.Eth
            .GetContractQueryHandler<ExportThingIdToSettlementProposalMessage>()
            .QueryAsync<ExportThingIdToSettlementProposalFunctionOutput>(_truquestAddress, new());

        var settlementProposalSubmitters = new List<SettlementProposalSubmitterQm>();
        for (int i = 0; i < result.ThingIds.Count; ++i)
        {
            settlementProposalSubmitters.Add(new()
            {
                ThingId = new Guid(result.ThingIds[i]),
                SettlementProposalId = new Guid(result.SettlementProposals[i].Id),
                Submitter = result.SettlementProposals[i].Submitter
            });
        }

        return settlementProposalSubmitters;
    }

    public async Task<IEnumerable<ThingValidationVerifierLotteryQm>> ExportThingValidationVerifierLotteryData()
    {
        var result = await _web3.Eth
            .GetContractQueryHandler<ExportThingValidationVerifierLotteryDataMessage>()
            .QueryAsync<ExportThingValidationVerifierLotteryDataFunctionOutput>(_thingValidationVerifierLotteryAddress, new());

        var lotteries = new List<ThingValidationVerifierLotteryQm>();
        for (int i = 0; i < result.ThingIds.Count; ++i)
        {
            lotteries.Add(new()
            {
                ThingId = new Guid(result.ThingIds[i]),
                OrchestratorCommitment = new()
                {
                    DataHash = result.OrchestratorCommitments[i].DataHash.ToHex(prefix: true),
                    UserXorDataHash = result.OrchestratorCommitments[i].UserXorDataHash.ToHex(prefix: true),
                    Block = (long)result.OrchestratorCommitments[i].Block
                },
                Participants = result.Participants[i]
                    .Select((p, i) => (Participant: p, Index: i))
                    .Join(
                        result.BlockNumbers[i].Select((b, i) => (BlockNumber: b, Index: i)),
                        pe => pe.Index,
                        be => be.Index,
                        (pe, be) => new LotteryParticipantQm
                        {
                            Address = pe.Participant,
                            BlockNumber = (long)be.BlockNumber
                        }
                    )
            });
        }

        return lotteries;
    }

    public async Task<IEnumerable<ThingValidationPollQm>> ExportThingValidationPollData()
    {
        var result = await _web3.Eth
            .GetContractQueryHandler<ExportThingValidationPollDataMessage>()
            .QueryAsync<ExportThingValidationPollDataFunctionOutput>(_thingValidationPollAddress, new());

        var polls = new List<ThingValidationPollQm>();
        for (int i = 0; i < result.ThingIds.Count; ++i)
        {
            polls.Add(new()
            {
                ThingId = new Guid(result.ThingIds[i]),
                InitBlockNumber = (long)result.InitBlockNumbers[i],
                Verifiers = result.Verifiers[i]
            });
        }

        return polls;
    }

    public async Task<IEnumerable<SettlementProposalAssessmentVerifierLotteryQm>> ExportSettlementProposalAssessmentVerifierLotteryData()
    {
        var result = await _web3.Eth
            .GetContractQueryHandler<ExportSettlementProposalAssessmentVerifierLotteryDataMessage>()
            .QueryAsync<ExportSettlementProposalAssessmentVerifierLotteryDataFunctionOutput>(
                _settlementProposalAssessmentVerifierLotteryAddress,
                new()
            );

        var lotteries = new List<SettlementProposalAssessmentVerifierLotteryQm>();
        for (int i = 0; i < result.ThingProposalIds.Count; ++i)
        {
            lotteries.Add(new()
            {
                ThingId = new Guid(result.ThingProposalIds[i].Take(16).ToArray()),
                SettlementProposalId = new Guid(result.ThingProposalIds[i].Skip(16).ToArray()),
                OrchestratorCommitment = new()
                {
                    DataHash = result.OrchestratorCommitments[i].DataHash.ToHex(prefix: true),
                    UserXorDataHash = result.OrchestratorCommitments[i].UserXorDataHash.ToHex(prefix: true),
                    Block = (long)result.OrchestratorCommitments[i].Block
                },
                Participants = result.Participants[i]
                    .Select((p, i) => (Participant: p, Index: i))
                    .Join(
                        result.BlockNumbers[i].Take(result.Participants[i].Count).Select((b, i) => (BlockNumber: b, Index: i)),
                        pe => pe.Index,
                        be => be.Index,
                        (pe, be) => new LotteryParticipantQm
                        {
                            Address = pe.Participant,
                            BlockNumber = (long)be.BlockNumber
                        }
                    ),
                Claimants = result.Claimants[i]
                    .Select((c, i) => (Claimant: c, Index: i))
                    .Join(
                        result.BlockNumbers[i].Skip(result.Participants[i].Count).Select((b, i) => (BlockNumber: b, Index: i)),
                        ce => ce.Index,
                        be => be.Index,
                        (ce, be) => new LotteryParticipantQm
                        {
                            Address = ce.Claimant,
                            BlockNumber = (long)be.BlockNumber
                        }
                    )
            });
        }

        return lotteries;
    }

    public async Task<IEnumerable<SettlementProposalAssessmentPollQm>> ExportSettlementProposalAssessmentPollData()
    {
        var result = await _web3.Eth
            .GetContractQueryHandler<ExportSettlementProposalAssessmentPollDataMessage>()
            .QueryAsync<ExportSettlementProposalAssessmentPollDataFunctionOutput>(
                _settlementProposalAssessmentPollAddress,
                new()
            );

        var polls = new List<SettlementProposalAssessmentPollQm>();
        for (int i = 0; i < result.ThingProposalIds.Count; ++i)
        {
            polls.Add(new()
            {
                ThingId = new Guid(result.ThingProposalIds[i].Take(16).ToArray()),
                SettlementProposalId = new Guid(result.ThingProposalIds[i].Skip(16).ToArray()),
                InitBlockNumber = (long)result.InitBlockNumbers[i],
                Verifiers = result.Verifiers[i]
            });
        }

        return polls;
    }
}
