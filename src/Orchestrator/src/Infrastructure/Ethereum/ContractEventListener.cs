using System.Reflection;
using System.Diagnostics;
using System.Threading.Channels;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using GoThataway;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.BlockchainProcessing.Processor;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.BlockchainProcessing.ProgressRepositories;

using Application.Common.Interfaces;
using AppEvents = Application.Ethereum.Events;

using Infrastructure.Ethereum.Events;
using ThingValidationVerifierLottery = Infrastructure.Ethereum.Events.ThingValidationVerifierLottery;
using SettlementProposalAssessmentVerifierLottery = Infrastructure.Ethereum.Events.SettlementProposalAssessmentVerifierLottery;
using ThingValidationPoll = Infrastructure.Ethereum.Events.ThingValidationPoll;
using SettlementProposalAssessmentPoll = Infrastructure.Ethereum.Events.SettlementProposalAssessmentPoll;

namespace Infrastructure.Ethereum;

internal class ContractEventListener : IContractEventListener
{
    private readonly ILogger<ContractEventListener> _logger;
    private readonly IBlockProgressRepository _blockProgressRepository;

    private readonly Web3 _web3;
    private readonly uint _blockConfirmations;
    private readonly string _truQuestAddress;
    private readonly string _thingValidationVerifierLotteryAddress;
    private readonly string _thingValidationPollAddress;
    private readonly string _settlementProposalAssessmentVerifierLotteryAddress;
    private readonly string _settlementProposalAssessmentPollAddress;

    private readonly ChannelReader<(IEventLog, TaskCompletionSource)> _stream;
    private readonly ChannelWriter<(IEventLog, TaskCompletionSource)> _sink;

    public ContractEventListener(
        IConfiguration configuration,
        ILogger<ContractEventListener> logger,
        IBlockProgressRepository blockProgressRepository
    )
    {
        _logger = logger;
        _blockProgressRepository = blockProgressRepository;

        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]);
        _blockConfirmations = configuration.GetValue<uint>($"Ethereum:Networks:{network}:BlockConfirmations");
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
        _thingValidationVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationVerifierLottery:Address"]!;
        _thingValidationPollAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationPoll:Address"]!;
        _settlementProposalAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentVerifierLottery:Address"]!;
        _settlementProposalAssessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentPoll:Address"]!;

        var channel = Channel.CreateUnbounded<(IEventLog, TaskCompletionSource)>(
            new UnboundedChannelOptions
            {
                SingleReader = true
            }
        );
        _stream = channel.Reader;
        _sink = channel.Writer;
    }

    public async IAsyncEnumerable<IEvent> GetNext([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        new Thread(async () =>
        {
            async Task WriteToChannel<T>(EventLog<T> @event) where T : IEventDTO
            {
                var tcs = new TaskCompletionSource();
                await _sink.WriteAsync((@event, tcs));
                await tcs.Task;
                // @@??: Check that IBlockProgressRepository updates only once this handler runs to completion.
            }

            // @@TODO: Use reflection to create and register handlers.

            var eventHandlers = new ProcessorHandler<FilterLog>[]
            {
                new EventLogProcessorHandler<ThingFundedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingValidationVerifierLottery.LotteryInitializedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingValidationVerifierLottery.JoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingValidationVerifierLottery.LotteryClosedWithSuccessEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingValidationVerifierLottery.LotteryClosedInFailureEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingValidationPoll.CastedVoteEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingValidationPoll.CastedVoteWithReasonEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingValidationPoll.PollFinalizedEvent>(WriteToChannel),
                new EventLogProcessorHandler<SettlementProposalFundedEvent>(WriteToChannel),
                new EventLogProcessorHandler<SettlementProposalAssessmentVerifierLottery.LotteryInitializedEvent>(WriteToChannel),
                new EventLogProcessorHandler<SettlementProposalAssessmentVerifierLottery.ClaimedLotterySpotEvent>(WriteToChannel),
                new EventLogProcessorHandler<SettlementProposalAssessmentVerifierLottery.JoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<SettlementProposalAssessmentVerifierLottery.LotteryClosedWithSuccessEvent>(WriteToChannel),
                // @@TODO: LotteryClosedInFailure
                new EventLogProcessorHandler<SettlementProposalAssessmentPoll.CastedVoteEvent>(WriteToChannel),
                new EventLogProcessorHandler<SettlementProposalAssessmentPoll.CastedVoteWithReasonEvent>(WriteToChannel),
                new EventLogProcessorHandler<SettlementProposalAssessmentPoll.PollFinalizedEvent>(WriteToChannel),
            };

            var contractFilter = new NewFilterInput
            {
                Address = new[]
                {
                    _truQuestAddress,
                    _thingValidationVerifierLotteryAddress,
                    _thingValidationPollAddress,
                    _settlementProposalAssessmentVerifierLotteryAddress,
                    _settlementProposalAssessmentPollAddress
                }
            };

            var logProcessor = _web3.Processing.Logs.CreateProcessor(
                logProcessors: eventHandlers,
                filter: contractFilter,
                minimumBlockConfirmations: _blockConfirmations,
                blockProgressRepository: _blockProgressRepository,
                log: _logger
            );

            try
            {
                await logProcessor.ExecuteAsync(cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _sink.Complete();
            }
        }).Start();

        await foreach (var (@event, tcs) in _stream.ReadAllAsync())
        {
#if DEBUG
            var eventProp = @event.GetType().GetProperty("Event")!;
            var parsedEvent = eventProp.GetValue(@event)!;
            var addressProps = parsedEvent.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttribute<ParameterAttribute>()!.Type == "address");

            var addressUtil = AddressUtil.Current;
            foreach (var addressProp in addressProps)
            {
                var address = (string)addressProp.GetValue(parsedEvent)!;
                Debug.Assert(
                    addressUtil.IsValidEthereumAddressHexFormat(address) &&
                    addressUtil.IsChecksumAddress(address)
                );
            }

            var addressArrayProps = parsedEvent.GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttribute<ParameterAttribute>()!.Type == "address[]");

            foreach (var addressArrayProp in addressArrayProps)
            {
                var addresses = (List<string>)addressArrayProp.GetValue(parsedEvent)!;
                foreach (var address in addresses)
                {
                    Debug.Assert(
                        addressUtil.IsValidEthereumAddressHexFormat(address) &&
                        addressUtil.IsChecksumAddress(address)
                    );
                }
            }
#endif

            if (@event is EventLog<ThingFundedEvent> thingFundedEvent)
            {
                yield return new AppEvents.ThingFunded.ThingFundedEvent
                {
                    BlockNumber = (long)thingFundedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingFundedEvent.Log.TransactionIndex.Value,
                    TxnHash = thingFundedEvent.Log.TransactionHash,
                    LogIndex = (int)thingFundedEvent.Log.LogIndex.Value,
                    ThingId = thingFundedEvent.Event.ThingId,
                    WalletAddress = thingFundedEvent.Event.User,
                    Stake = (decimal)thingFundedEvent.Event.Stake
                };
            }
            else if (@event is EventLog<ThingValidationVerifierLottery.LotteryInitializedEvent> thingValidationVerifierLotteryInitializedEvent)
            {
                yield return new AppEvents.ThingValidationVerifierLottery.LotteryInitialized.LotteryInitializedEvent
                {
                    BlockNumber = (long)thingValidationVerifierLotteryInitializedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingValidationVerifierLotteryInitializedEvent.Log.TransactionIndex.Value,
                    TxnHash = thingValidationVerifierLotteryInitializedEvent.Log.TransactionHash,
                    LogIndex = (int)thingValidationVerifierLotteryInitializedEvent.Log.LogIndex.Value,
                    L1BlockNumber = (long)thingValidationVerifierLotteryInitializedEvent.Event.L1BlockNumber,
                    ThingId = thingValidationVerifierLotteryInitializedEvent.Event.ThingId,
                    DataHash = thingValidationVerifierLotteryInitializedEvent.Event.DataHash,
                    UserXorDataHash = thingValidationVerifierLotteryInitializedEvent.Event.UserXorDataHash
                };
            }
            else if (@event is EventLog<ThingValidationVerifierLottery.JoinedLotteryEvent> joinedThingValidationVerifierLotteryEvent)
            {
                yield return new AppEvents.ThingValidationVerifierLottery.JoinedLottery.JoinedLotteryEvent
                {
                    BlockNumber = (long)joinedThingValidationVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedThingValidationVerifierLotteryEvent.Log.TransactionIndex.Value,
                    TxnHash = joinedThingValidationVerifierLotteryEvent.Log.TransactionHash,
                    LogIndex = (int)joinedThingValidationVerifierLotteryEvent.Log.LogIndex.Value,
                    ThingId = joinedThingValidationVerifierLotteryEvent.Event.ThingId,
                    WalletAddress = joinedThingValidationVerifierLotteryEvent.Event.User,
                    UserData = joinedThingValidationVerifierLotteryEvent.Event.UserData,
                    L1BlockNumber = (long)joinedThingValidationVerifierLotteryEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<ThingValidationVerifierLottery.LotteryClosedWithSuccessEvent> thingValidationVerifierLotteryClosedWithSuccessEvent)
            {
                yield return new AppEvents.ThingValidationVerifierLottery.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent
                {
                    BlockNumber = (long)thingValidationVerifierLotteryClosedWithSuccessEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingValidationVerifierLotteryClosedWithSuccessEvent.Log.TransactionIndex.Value,
                    TxnHash = thingValidationVerifierLotteryClosedWithSuccessEvent.Log.TransactionHash,
                    LogIndex = (int)thingValidationVerifierLotteryClosedWithSuccessEvent.Log.LogIndex.Value,
                    ThingId = thingValidationVerifierLotteryClosedWithSuccessEvent.Event.ThingId,
                    Orchestrator = thingValidationVerifierLotteryClosedWithSuccessEvent.Event.Orchestrator,
                    Data = thingValidationVerifierLotteryClosedWithSuccessEvent.Event.Data,
                    UserXorData = thingValidationVerifierLotteryClosedWithSuccessEvent.Event.UserXorData,
                    HashOfL1EndBlock = thingValidationVerifierLotteryClosedWithSuccessEvent.Event.HashOfL1EndBlock,
                    Nonce = (long)thingValidationVerifierLotteryClosedWithSuccessEvent.Event.Nonce,
                    WinnerWalletAddresses = thingValidationVerifierLotteryClosedWithSuccessEvent.Event.Winners
                };
            }
            else if (@event is EventLog<ThingValidationVerifierLottery.LotteryClosedInFailureEvent> thingValidationVerifierLotteryClosedInFailureEvent)
            {
                yield return new AppEvents.ThingValidationVerifierLottery.LotteryClosedInFailure.LotteryClosedInFailureEvent
                {
                    BlockNumber = (long)thingValidationVerifierLotteryClosedInFailureEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingValidationVerifierLotteryClosedInFailureEvent.Log.TransactionIndex.Value,
                    TxnHash = thingValidationVerifierLotteryClosedInFailureEvent.Log.TransactionHash,
                    LogIndex = (int)thingValidationVerifierLotteryClosedInFailureEvent.Log.LogIndex.Value,
                    ThingId = thingValidationVerifierLotteryClosedInFailureEvent.Event.ThingId,
                    RequiredNumVerifiers = thingValidationVerifierLotteryClosedInFailureEvent.Event.RequiredNumVerifiers,
                    JoinedNumVerifiers = thingValidationVerifierLotteryClosedInFailureEvent.Event.JoinedNumVerifiers
                };
            }
            else if (@event is EventLog<ThingValidationPoll.CastedVoteEvent> castedThingValidationPollVoteEvent)
            {
                yield return new AppEvents.ThingValidationPoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedThingValidationPollVoteEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedThingValidationPollVoteEvent.Log.TransactionIndex.Value,
                    TxnHash = castedThingValidationPollVoteEvent.Log.TransactionHash,
                    LogIndex = (int)castedThingValidationPollVoteEvent.Log.LogIndex.Value,
                    ThingId = castedThingValidationPollVoteEvent.Event.ThingId,
                    WalletAddress = castedThingValidationPollVoteEvent.Event.User,
                    Vote = castedThingValidationPollVoteEvent.Event.Vote,
                    L1BlockNumber = (long)castedThingValidationPollVoteEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<ThingValidationPoll.CastedVoteWithReasonEvent> castedThingValidationPollVoteWithReasonEvent)
            {
                yield return new AppEvents.ThingValidationPoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedThingValidationPollVoteWithReasonEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedThingValidationPollVoteWithReasonEvent.Log.TransactionIndex.Value,
                    TxnHash = castedThingValidationPollVoteWithReasonEvent.Log.TransactionHash,
                    LogIndex = (int)castedThingValidationPollVoteWithReasonEvent.Log.LogIndex.Value,
                    ThingId = castedThingValidationPollVoteWithReasonEvent.Event.ThingId,
                    WalletAddress = castedThingValidationPollVoteWithReasonEvent.Event.User,
                    Vote = castedThingValidationPollVoteWithReasonEvent.Event.Vote,
                    Reason = castedThingValidationPollVoteWithReasonEvent.Event.Reason,
                    L1BlockNumber = (long)castedThingValidationPollVoteWithReasonEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<ThingValidationPoll.PollFinalizedEvent> thingValidationPollFinalizedEvent)
            {
                yield return new AppEvents.ThingValidationPoll.PollFinalized.PollFinalizedEvent
                {
                    BlockNumber = (long)thingValidationPollFinalizedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingValidationPollFinalizedEvent.Log.TransactionIndex.Value,
                    TxnHash = thingValidationPollFinalizedEvent.Log.TransactionHash,
                    LogIndex = (int)thingValidationPollFinalizedEvent.Log.LogIndex.Value,
                    ThingId = thingValidationPollFinalizedEvent.Event.ThingId,
                    Decision = thingValidationPollFinalizedEvent.Event.Decision,
                    VoteAggIpfsCid = thingValidationPollFinalizedEvent.Event.VoteAggIpfsCid,
                    RewardedVerifiers = thingValidationPollFinalizedEvent.Event.RewardedVerifiers,
                    SlashedVerifiers = thingValidationPollFinalizedEvent.Event.SlashedVerifiers
                };
            }
            else if (@event is EventLog<SettlementProposalFundedEvent> settlementProposalFundedEvent)
            {
                yield return new AppEvents.SettlementProposalFunded.SettlementProposalFundedEvent
                {
                    BlockNumber = (long)settlementProposalFundedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)settlementProposalFundedEvent.Log.TransactionIndex.Value,
                    TxnHash = settlementProposalFundedEvent.Log.TransactionHash,
                    LogIndex = (int)settlementProposalFundedEvent.Log.LogIndex.Value,
                    ThingId = settlementProposalFundedEvent.Event.ThingId,
                    SettlementProposalId = settlementProposalFundedEvent.Event.SettlementProposalId,
                    WalletAddress = settlementProposalFundedEvent.Event.User,
                    Stake = (decimal)settlementProposalFundedEvent.Event.Stake
                };
            }
            else if (
                @event is EventLog<SettlementProposalAssessmentVerifierLottery.LotteryInitializedEvent>
                    settlementProposalAssessmentVerifierLotteryInitializedEvent
            )
            {
                yield return new AppEvents.SettlementProposalAssessmentVerifierLottery.LotteryInitialized.LotteryInitializedEvent
                {
                    BlockNumber = (long)settlementProposalAssessmentVerifierLotteryInitializedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)settlementProposalAssessmentVerifierLotteryInitializedEvent.Log.TransactionIndex.Value,
                    TxnHash = settlementProposalAssessmentVerifierLotteryInitializedEvent.Log.TransactionHash,
                    LogIndex = (int)settlementProposalAssessmentVerifierLotteryInitializedEvent.Log.LogIndex.Value,
                    L1BlockNumber = (long)settlementProposalAssessmentVerifierLotteryInitializedEvent.Event.L1BlockNumber,
                    ThingId = settlementProposalAssessmentVerifierLotteryInitializedEvent.Event.ThingId,
                    SettlementProposalId = settlementProposalAssessmentVerifierLotteryInitializedEvent.Event.SettlementProposalId,
                    DataHash = settlementProposalAssessmentVerifierLotteryInitializedEvent.Event.DataHash,
                    UserXorDataHash = settlementProposalAssessmentVerifierLotteryInitializedEvent.Event.UserXorDataHash,
                };
            }
            else if (
                @event is EventLog<SettlementProposalAssessmentVerifierLottery.ClaimedLotterySpotEvent>
                    claimedSettlementProposalAssessmentVerifierLotterySpotEvent
            )
            {
                yield return new AppEvents.SettlementProposalAssessmentVerifierLottery.ClaimedLotterySpot.ClaimedLotterySpotEvent
                {
                    BlockNumber = (long)claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Log.TransactionIndex.Value,
                    TxnHash = claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Log.TransactionHash,
                    LogIndex = (int)claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Log.LogIndex.Value,
                    ThingId = claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Event.ThingId,
                    SettlementProposalId = claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Event.SettlementProposalId,
                    WalletAddress = claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Event.User,
                    L1BlockNumber = (long)claimedSettlementProposalAssessmentVerifierLotterySpotEvent.Event.L1BlockNumber
                };
            }
            else if (
                @event is EventLog<SettlementProposalAssessmentVerifierLottery.JoinedLotteryEvent>
                    joinedSettlementProposalAssessmentVerifierLotteryEvent
            )
            {
                yield return new AppEvents.SettlementProposalAssessmentVerifierLottery.JoinedLottery.JoinedLotteryEvent
                {
                    BlockNumber = (long)joinedSettlementProposalAssessmentVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedSettlementProposalAssessmentVerifierLotteryEvent.Log.TransactionIndex.Value,
                    TxnHash = joinedSettlementProposalAssessmentVerifierLotteryEvent.Log.TransactionHash,
                    LogIndex = (int)joinedSettlementProposalAssessmentVerifierLotteryEvent.Log.LogIndex.Value,
                    ThingId = joinedSettlementProposalAssessmentVerifierLotteryEvent.Event.ThingId,
                    SettlementProposalId = joinedSettlementProposalAssessmentVerifierLotteryEvent.Event.SettlementProposalId,
                    WalletAddress = joinedSettlementProposalAssessmentVerifierLotteryEvent.Event.User,
                    UserData = joinedSettlementProposalAssessmentVerifierLotteryEvent.Event.UserData,
                    L1BlockNumber = (long)joinedSettlementProposalAssessmentVerifierLotteryEvent.Event.L1BlockNumber
                };
            }
            else if (
                @event is EventLog<SettlementProposalAssessmentVerifierLottery.LotteryClosedWithSuccessEvent>
                    settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent
            )
            {
                yield return new AppEvents.SettlementProposalAssessmentVerifierLottery.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent
                {
                    BlockNumber = (long)settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Log.TransactionIndex.Value,
                    TxnHash = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Log.TransactionHash,
                    LogIndex = (int)settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Log.LogIndex.Value,
                    ThingId = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.ThingId,
                    SettlementProposalId = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.SettlementProposalId,
                    Orchestrator = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Orchestrator,
                    Data = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Data,
                    UserXorData = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.UserXorData,
                    HashOfL1EndBlock = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.HashOfL1EndBlock,
                    Nonce = (long)settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Nonce,
                    ClaimantWalletAddresses = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Claimants,
                    WinnerWalletAddresses = settlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Winners
                };
            }
            else if (@event is EventLog<SettlementProposalAssessmentPoll.CastedVoteEvent> castedSettlementProposalAssessmentPollVoteEvent)
            {
                yield return new AppEvents.SettlementProposalAssessmentPoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedSettlementProposalAssessmentPollVoteEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedSettlementProposalAssessmentPollVoteEvent.Log.TransactionIndex.Value,
                    TxnHash = castedSettlementProposalAssessmentPollVoteEvent.Log.TransactionHash,
                    LogIndex = (int)castedSettlementProposalAssessmentPollVoteEvent.Log.LogIndex.Value,
                    ThingId = castedSettlementProposalAssessmentPollVoteEvent.Event.ThingId,
                    SettlementProposalId = castedSettlementProposalAssessmentPollVoteEvent.Event.SettlementProposalId,
                    WalletAddress = castedSettlementProposalAssessmentPollVoteEvent.Event.User,
                    Vote = castedSettlementProposalAssessmentPollVoteEvent.Event.Vote,
                    L1BlockNumber = (long)castedSettlementProposalAssessmentPollVoteEvent.Event.L1BlockNumber
                };
            }
            else if (
                @event is EventLog<SettlementProposalAssessmentPoll.CastedVoteWithReasonEvent>
                    castedSettlementProposalAssessmentPollVoteWithReasonEvent
            )
            {
                yield return new AppEvents.SettlementProposalAssessmentPoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedSettlementProposalAssessmentPollVoteWithReasonEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedSettlementProposalAssessmentPollVoteWithReasonEvent.Log.TransactionIndex.Value,
                    TxnHash = castedSettlementProposalAssessmentPollVoteWithReasonEvent.Log.TransactionHash,
                    LogIndex = (int)castedSettlementProposalAssessmentPollVoteWithReasonEvent.Log.LogIndex.Value,
                    ThingId = castedSettlementProposalAssessmentPollVoteWithReasonEvent.Event.ThingId,
                    SettlementProposalId = castedSettlementProposalAssessmentPollVoteWithReasonEvent.Event.SettlementProposalId,
                    WalletAddress = castedSettlementProposalAssessmentPollVoteWithReasonEvent.Event.User,
                    Vote = castedSettlementProposalAssessmentPollVoteWithReasonEvent.Event.Vote,
                    Reason = castedSettlementProposalAssessmentPollVoteWithReasonEvent.Event.Reason,
                    L1BlockNumber = (long)castedSettlementProposalAssessmentPollVoteWithReasonEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<SettlementProposalAssessmentPoll.PollFinalizedEvent> settlementProposalAssessmentPollFinalizedEvent)
            {
                yield return new AppEvents.SettlementProposalAssessmentPoll.PollFinalized.PollFinalizedEvent
                {
                    BlockNumber = (long)settlementProposalAssessmentPollFinalizedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)settlementProposalAssessmentPollFinalizedEvent.Log.TransactionIndex.Value,
                    TxnHash = settlementProposalAssessmentPollFinalizedEvent.Log.TransactionHash,
                    LogIndex = (int)settlementProposalAssessmentPollFinalizedEvent.Log.LogIndex.Value,
                    ThingId = settlementProposalAssessmentPollFinalizedEvent.Event.ThingId,
                    SettlementProposalId = settlementProposalAssessmentPollFinalizedEvent.Event.SettlementProposalId,
                    Decision = settlementProposalAssessmentPollFinalizedEvent.Event.Decision,
                    VoteAggIpfsCid = settlementProposalAssessmentPollFinalizedEvent.Event.VoteAggIpfsCid,
                    RewardedVerifiers = settlementProposalAssessmentPollFinalizedEvent.Event.RewardedVerifiers,
                    SlashedVerifiers = settlementProposalAssessmentPollFinalizedEvent.Event.SlashedVerifiers
                };
            }

            tcs.SetResult();
        }
    }
}
