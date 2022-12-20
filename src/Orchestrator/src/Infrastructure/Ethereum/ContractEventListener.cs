using System.Threading.Channels;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using MediatR;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.BlockchainProcessing.Processor;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.BlockchainProcessing.ProgressRepositories;

using Application.Common.Interfaces;
using AppEvents = Application.Ethereum.Events;

using Infrastructure.Ethereum.Events;
using ThingAssessmentVerifierLottery = Infrastructure.Ethereum.Events.ThingAssessmentVerifierLottery;

namespace Infrastructure.Ethereum;

internal class ContractEventListener : IContractEventListener
{
    private readonly ILogger<ContractEventListener> _logger;
    private readonly IBlockProgressRepository _blockProgressRepository;

    private readonly Web3 _web3;
    private readonly uint _blockConfirmations;
    private readonly string _truQuestAddress;
    private readonly string _verifierLotteryAddress;
    private readonly string _acceptancePollAddress;
    private readonly string _thingAssessmentVerifierLotteryAddress;

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
        _verifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
        _thingAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!;

        var channel = Channel.CreateUnbounded<(IEventLog, TaskCompletionSource)>(
            new UnboundedChannelOptions
            {
                SingleReader = true
            }
        );
        _stream = channel.Reader;
        _sink = channel.Writer;
    }

    public async IAsyncEnumerable<INotification> GetNext([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        new Thread(async () =>
        {
            async Task WriteToChannel<T>(EventLog<T> @event) where T : IEventDTO
            {
                var tcs = new TaskCompletionSource();
                await _sink.WriteAsync((@event, tcs));
                await tcs.Task;
            }

            // @@TODO: Use reflection to create and register handlers.

            var eventHandlers = new ProcessorHandler<FilterLog>[]
            {
                new EventLogProcessorHandler<ThingFundedEvent>(WriteToChannel),
                new EventLogProcessorHandler<PreJoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<JoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<LotteryClosedWithSuccessEvent>(WriteToChannel),
                new EventLogProcessorHandler<CastedVoteEvent>(WriteToChannel),
                new EventLogProcessorHandler<CastedVoteWithReasonEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSettlementProposalFundedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingAssessmentVerifierLottery.LotterySpotClaimedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingAssessmentVerifierLottery.PreJoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingAssessmentVerifierLottery.JoinedLotteryEvent>(WriteToChannel)
            };

            var contractFilter = new NewFilterInput
            {
                Address = new[]
                {
                    _truQuestAddress,
                    _verifierLotteryAddress,
                    _acceptancePollAddress,
                    _thingAssessmentVerifierLotteryAddress
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
            if (@event is EventLog<ThingFundedEvent> thingFundedEvent)
            {
                yield return new AppEvents.ThingFunded.ThingFundedEvent
                {
                    BlockNumber = (long)thingFundedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingFundedEvent.Log.TransactionIndex.Value,
                    ThingIdHash = thingFundedEvent.Event.ThingIdHash.Substring(2),
                    UserId = thingFundedEvent.Event.UserId.Substring(2).ToLower(),
                    Stake = (decimal)thingFundedEvent.Event.Stake
                };
            }
            else if (@event is EventLog<PreJoinedLotteryEvent> preJoinedVerifierLotteryEvent)
            {
                yield return new AppEvents.Lottery.PreJoinedVerifierLottery.PreJoinedVerifierLotteryEvent
                {
                    BlockNumber = (long)preJoinedVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)preJoinedVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingIdHash = preJoinedVerifierLotteryEvent.Event.ThingIdHash.Substring(2),
                    UserId = preJoinedVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    DataHash = preJoinedVerifierLotteryEvent.Event.DataHash
                };
            }
            else if (@event is EventLog<JoinedLotteryEvent> joinedVerifierLotteryEvent)
            {
                yield return new AppEvents.Lottery.JoinedVerifierLottery.JoinedVerifierLotteryEvent
                {
                    BlockNumber = (long)joinedVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingIdHash = joinedVerifierLotteryEvent.Event.ThingIdHash.Substring(2),
                    UserId = joinedVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    Nonce = joinedVerifierLotteryEvent.Event.Nonce
                };
            }
            else if (@event is EventLog<LotteryClosedWithSuccessEvent> verifierLotteryClosedWithSuccessEvent)
            {
                yield return new AppEvents.Lottery.VerifierLotteryClosedWithSuccess.VerifierLotteryClosedWithSuccessEvent
                {
                    BlockNumber = (long)verifierLotteryClosedWithSuccessEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)verifierLotteryClosedWithSuccessEvent.Log.TransactionIndex.Value,
                    ThingIdHash = verifierLotteryClosedWithSuccessEvent.Event.ThingIdHash.Substring(2),
                    Nonce = (decimal)verifierLotteryClosedWithSuccessEvent.Event.Nonce,
                    WinnerIds = verifierLotteryClosedWithSuccessEvent.Event.WinnerIds
                        .Select(id => id.Substring(2).ToLower())
                        .ToList()
                };
            }
            else if (@event is EventLog<CastedVoteEvent> castedAcceptancePollVoteEvent)
            {
                yield return new AppEvents.AcceptancePoll.CastedAcceptancePollVote.CastedAcceptancePollVoteEvent
                {
                    BlockNumber = (long)castedAcceptancePollVoteEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedAcceptancePollVoteEvent.Log.TransactionIndex.Value,
                    ThingIdHash = castedAcceptancePollVoteEvent.Event.ThingIdHash.Substring(2),
                    UserId = castedAcceptancePollVoteEvent.Event.UserId.Substring(2).ToLower(),
                    Vote = castedAcceptancePollVoteEvent.Event.Vote
                };
            }
            else if (@event is EventLog<CastedVoteWithReasonEvent> castedAcceptancePollVoteWithReasonEvent)
            {
                yield return new AppEvents.AcceptancePoll.CastedAcceptancePollVote.CastedAcceptancePollVoteEvent
                {
                    BlockNumber = (long)castedAcceptancePollVoteWithReasonEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedAcceptancePollVoteWithReasonEvent.Log.TransactionIndex.Value,
                    ThingIdHash = castedAcceptancePollVoteWithReasonEvent.Event.ThingIdHash.Substring(2),
                    UserId = castedAcceptancePollVoteWithReasonEvent.Event.UserId.Substring(2).ToLower(),
                    Vote = castedAcceptancePollVoteWithReasonEvent.Event.Vote,
                    Reason = castedAcceptancePollVoteWithReasonEvent.Event.Reason
                };
            }
            else if (@event is EventLog<ThingSettlementProposalFundedEvent> thingSettlementProposalFundedEvent)
            {
                yield return new AppEvents.ThingSettlementProposalFunded.ThingSettlementProposalFundedEvent
                {
                    BlockNumber = (long)thingSettlementProposalFundedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingSettlementProposalFundedEvent.Log.TransactionIndex.Value,
                    ThingIdHash = thingSettlementProposalFundedEvent.Event.ThingIdHash.Substring(2),
                    SettlementProposalIdHash = thingSettlementProposalFundedEvent.Event.SettlementProposalIdHash.Substring(2),
                    UserId = thingSettlementProposalFundedEvent.Event.UserId.Substring(2).ToLower(),
                    Stake = (decimal)thingSettlementProposalFundedEvent.Event.Stake
                };
            }
            else if (@event is EventLog<ThingAssessmentVerifierLottery.PreJoinedLotteryEvent> preJoinedThingAssessmentVerifierLotteryEvent)
            {
                yield return new AppEvents.ThingAssessmentVerifierLottery.PreJoinedLottery.PreJoinedLotteryEvent
                {
                    BlockNumber = (long)preJoinedThingAssessmentVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)preJoinedThingAssessmentVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingIdHash = preJoinedThingAssessmentVerifierLotteryEvent.Event.ThingIdHash.Substring(2),
                    SettlementProposalIdHash = preJoinedThingAssessmentVerifierLotteryEvent.Event.SettlementProposalIdHash.Substring(2),
                    UserId = preJoinedThingAssessmentVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    DataHash = preJoinedThingAssessmentVerifierLotteryEvent.Event.DataHash
                };
            }
            else if (@event is EventLog<ThingAssessmentVerifierLottery.JoinedLotteryEvent> joinedThingAssessmentVerifierLotteryEvent)
            {
                yield return new AppEvents.ThingAssessmentVerifierLottery.JoinedLottery.JoinedLotteryEvent
                {
                    BlockNumber = (long)joinedThingAssessmentVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedThingAssessmentVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingIdHash = joinedThingAssessmentVerifierLotteryEvent.Event.ThingIdHash.Substring(2),
                    SettlementProposalIdHash = joinedThingAssessmentVerifierLotteryEvent.Event.SettlementProposalIdHash.Substring(2),
                    UserId = joinedThingAssessmentVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    Nonce = joinedThingAssessmentVerifierLotteryEvent.Event.Nonce
                };
            }
            else if (@event is EventLog<ThingAssessmentVerifierLottery.LotterySpotClaimedEvent> thingAssessmentVerifierLotterySpotClaimedEvent)
            {
                yield return new AppEvents.ThingAssessmentVerifierLottery.LotterySpotClaimed.LotterySpotClaimedEvent
                {
                    BlockNumber = (long)thingAssessmentVerifierLotterySpotClaimedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingAssessmentVerifierLotterySpotClaimedEvent.Log.TransactionIndex.Value,
                    ThingIdHash = thingAssessmentVerifierLotterySpotClaimedEvent.Event.ThingIdHash.Substring(2),
                    SettlementProposalIdHash = thingAssessmentVerifierLotterySpotClaimedEvent.Event.SettlementProposalIdHash.Substring(2),
                    UserId = thingAssessmentVerifierLotterySpotClaimedEvent.Event.UserId.Substring(2).ToLower(),
                };
            }

            tcs.SetResult();
        }
    }
}