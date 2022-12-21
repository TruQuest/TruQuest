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
using ThingSubmissionVerifierLottery = Infrastructure.Ethereum.Events.ThingSubmissionVerifierLottery;
using ThingAssessmentVerifierLottery = Infrastructure.Ethereum.Events.ThingAssessmentVerifierLottery;

namespace Infrastructure.Ethereum;

internal class ContractEventListener : IContractEventListener
{
    private readonly ILogger<ContractEventListener> _logger;
    private readonly IBlockProgressRepository _blockProgressRepository;

    private readonly Web3 _web3;
    private readonly uint _blockConfirmations;
    private readonly string _truQuestAddress;
    private readonly string _thingSubmissionVerifierLotteryAddress;
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
        _thingSubmissionVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"]!;
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
                new EventLogProcessorHandler<ThingSubmissionVerifierLottery.PreJoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSubmissionVerifierLottery.JoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSubmissionVerifierLottery.LotteryClosedWithSuccessEvent>(WriteToChannel),
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
                    _thingSubmissionVerifierLotteryAddress,
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
                    ThingId = thingFundedEvent.Event.ThingId,
                    UserId = thingFundedEvent.Event.UserId.Substring(2).ToLower(),
                    Stake = (decimal)thingFundedEvent.Event.Stake
                };
            }
            else if (@event is EventLog<ThingSubmissionVerifierLottery.PreJoinedLotteryEvent> preJoinedThingSubmissionVerifierLotteryEvent)
            {
                yield return new AppEvents.ThingSubmissionVerifierLottery.PreJoinedLottery.PreJoinedLotteryEvent
                {
                    BlockNumber = (long)preJoinedThingSubmissionVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)preJoinedThingSubmissionVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingId = preJoinedThingSubmissionVerifierLotteryEvent.Event.ThingId,
                    UserId = preJoinedThingSubmissionVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    DataHash = preJoinedThingSubmissionVerifierLotteryEvent.Event.DataHash
                };
            }
            else if (@event is EventLog<ThingSubmissionVerifierLottery.JoinedLotteryEvent> joinedThingSubmissionVerifierLotteryEvent)
            {
                yield return new AppEvents.ThingSubmissionVerifierLottery.JoinedLottery.JoinedLotteryEvent
                {
                    BlockNumber = (long)joinedThingSubmissionVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedThingSubmissionVerifierLotteryEvent.Log.TransactionIndex.Value,
                    ThingId = joinedThingSubmissionVerifierLotteryEvent.Event.ThingId,
                    UserId = joinedThingSubmissionVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    Nonce = joinedThingSubmissionVerifierLotteryEvent.Event.Nonce
                };
            }
            else if (@event is EventLog<ThingSubmissionVerifierLottery.LotteryClosedWithSuccessEvent> thingSubmissionVerifierLotteryClosedWithSuccessEvent)
            {
                yield return new AppEvents.ThingSubmissionVerifierLottery.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent
                {
                    BlockNumber = (long)thingSubmissionVerifierLotteryClosedWithSuccessEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingSubmissionVerifierLotteryClosedWithSuccessEvent.Log.TransactionIndex.Value,
                    ThingId = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.ThingId,
                    Nonce = (decimal)thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.Nonce,
                    WinnerIds = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.WinnerIds
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
                    ThingId = castedAcceptancePollVoteEvent.Event.ThingId,
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
                    ThingId = castedAcceptancePollVoteWithReasonEvent.Event.ThingId,
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
                    ThingId = thingSettlementProposalFundedEvent.Event.ThingId,
                    SettlementProposalId = thingSettlementProposalFundedEvent.Event.SettlementProposalId,
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
                    ThingId = preJoinedThingAssessmentVerifierLotteryEvent.Event.ThingId,
                    SettlementProposalId = preJoinedThingAssessmentVerifierLotteryEvent.Event.SettlementProposalId,
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
                    ThingId = joinedThingAssessmentVerifierLotteryEvent.Event.ThingId,
                    SettlementProposalId = joinedThingAssessmentVerifierLotteryEvent.Event.SettlementProposalId,
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
                    ThingId = thingAssessmentVerifierLotterySpotClaimedEvent.Event.ThingId,
                    SettlementProposalId = thingAssessmentVerifierLotterySpotClaimedEvent.Event.SettlementProposalId,
                    UserId = thingAssessmentVerifierLotterySpotClaimedEvent.Event.UserId.Substring(2).ToLower(),
                };
            }

            tcs.SetResult();
        }
    }
}