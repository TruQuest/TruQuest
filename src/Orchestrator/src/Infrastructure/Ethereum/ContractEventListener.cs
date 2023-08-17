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
using AcceptancePoll = Infrastructure.Ethereum.Events.AcceptancePoll;
using AssessmentPoll = Infrastructure.Ethereum.Events.AssessmentPoll;

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
    private readonly string _assessmentPollAddress;

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
        _assessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:AssessmentPoll:Address"]!;

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
                // @@??: Check that IBlockProgressRepository updates only once this handler runs to completion.
            }

            // @@TODO: Use reflection to create and register handlers.

            var eventHandlers = new ProcessorHandler<FilterLog>[]
            {
                new EventLogProcessorHandler<ThingFundedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSubmissionVerifierLottery.LotteryInitializedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSubmissionVerifierLottery.JoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSubmissionVerifierLottery.LotteryClosedWithSuccessEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSubmissionVerifierLottery.LotteryClosedInFailureEvent>(WriteToChannel),
                new EventLogProcessorHandler<AcceptancePoll.CastedVoteEvent>(WriteToChannel),
                new EventLogProcessorHandler<AcceptancePoll.CastedVoteWithReasonEvent>(WriteToChannel),
                new EventLogProcessorHandler<AcceptancePoll.PollFinalizedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingSettlementProposalFundedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingAssessmentVerifierLottery.LotteryInitializedEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingAssessmentVerifierLottery.ClaimedLotterySpotEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingAssessmentVerifierLottery.JoinedLotteryEvent>(WriteToChannel),
                new EventLogProcessorHandler<ThingAssessmentVerifierLottery.LotteryClosedWithSuccessEvent>(WriteToChannel),
                // @@TODO: LotteryClosedInFailure
                new EventLogProcessorHandler<AssessmentPoll.CastedVoteEvent>(WriteToChannel),
                new EventLogProcessorHandler<AssessmentPoll.CastedVoteWithReasonEvent>(WriteToChannel),
                new EventLogProcessorHandler<AssessmentPoll.PollFinalizedEvent>(WriteToChannel),
            };

            var contractFilter = new NewFilterInput
            {
                Address = new[]
                {
                    _truQuestAddress,
                    _thingSubmissionVerifierLotteryAddress,
                    _acceptancePollAddress,
                    _thingAssessmentVerifierLotteryAddress,
                    _assessmentPollAddress
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
                    TxnHash = thingFundedEvent.Log.TransactionHash,
                    ThingId = thingFundedEvent.Event.ThingId,
                    UserId = thingFundedEvent.Event.UserId.Substring(2).ToLower(),
                    Stake = (decimal)thingFundedEvent.Event.Stake
                };
            }
            else if (@event is EventLog<ThingSubmissionVerifierLottery.LotteryInitializedEvent> thingSubmissionVerifierLotteryInitializedEvent)
            {
                yield return new AppEvents.ThingSubmissionVerifierLottery.LotteryInitialized.LotteryInitializedEvent
                {
                    BlockNumber = (long)thingSubmissionVerifierLotteryInitializedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingSubmissionVerifierLotteryInitializedEvent.Log.TransactionIndex.Value,
                    TxnHash = thingSubmissionVerifierLotteryInitializedEvent.Log.TransactionHash,
                    ThingId = thingSubmissionVerifierLotteryInitializedEvent.Event.ThingId,
                    DataHash = thingSubmissionVerifierLotteryInitializedEvent.Event.DataHash,
                    UserXorDataHash = thingSubmissionVerifierLotteryInitializedEvent.Event.UserXorDataHash
                };
            }
            else if (@event is EventLog<ThingSubmissionVerifierLottery.JoinedLotteryEvent> joinedThingSubmissionVerifierLotteryEvent)
            {
                yield return new AppEvents.ThingSubmissionVerifierLottery.JoinedLottery.JoinedLotteryEvent
                {
                    BlockNumber = (long)joinedThingSubmissionVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedThingSubmissionVerifierLotteryEvent.Log.TransactionIndex.Value,
                    TxnHash = joinedThingSubmissionVerifierLotteryEvent.Log.TransactionHash,
                    ThingId = joinedThingSubmissionVerifierLotteryEvent.Event.ThingId,
                    UserId = joinedThingSubmissionVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    UserData = joinedThingSubmissionVerifierLotteryEvent.Event.UserData,
                    L1BlockNumber = (long)joinedThingSubmissionVerifierLotteryEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<ThingSubmissionVerifierLottery.LotteryClosedWithSuccessEvent> thingSubmissionVerifierLotteryClosedWithSuccessEvent)
            {
                yield return new AppEvents.ThingSubmissionVerifierLottery.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent
                {
                    BlockNumber = (long)thingSubmissionVerifierLotteryClosedWithSuccessEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingSubmissionVerifierLotteryClosedWithSuccessEvent.Log.TransactionIndex.Value,
                    TxnHash = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Log.TransactionHash,
                    ThingId = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.ThingId,
                    Orchestrator = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.Orchestrator.Substring(2).ToLower(),
                    Data = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.Data,
                    UserXorData = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.UserXorData,
                    HashOfL1EndBlock = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.HashOfL1EndBlock,
                    Nonce = (long)thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.Nonce,
                    WinnerIds = thingSubmissionVerifierLotteryClosedWithSuccessEvent.Event.WinnerIds
                        .Select(id => id.Substring(2).ToLower())
                        .ToList()
                };
            }
            else if (@event is EventLog<ThingSubmissionVerifierLottery.LotteryClosedInFailureEvent> thingSubmissionVerifierLotteryClosedInFailureEvent)
            {
                yield return new AppEvents.ThingSubmissionVerifierLottery.LotteryClosedInFailure.LotteryClosedInFailureEvent
                {
                    BlockNumber = (long)thingSubmissionVerifierLotteryClosedInFailureEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingSubmissionVerifierLotteryClosedInFailureEvent.Log.TransactionIndex.Value,
                    TxnHash = thingSubmissionVerifierLotteryClosedInFailureEvent.Log.TransactionHash,
                    ThingId = thingSubmissionVerifierLotteryClosedInFailureEvent.Event.ThingId,
                    RequiredNumVerifiers = thingSubmissionVerifierLotteryClosedInFailureEvent.Event.RequiredNumVerifiers,
                    JoinedNumVerifiers = thingSubmissionVerifierLotteryClosedInFailureEvent.Event.JoinedNumVerifiers
                };
            }
            else if (@event is EventLog<AcceptancePoll.CastedVoteEvent> castedAcceptancePollVoteEvent)
            {
                yield return new AppEvents.AcceptancePoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedAcceptancePollVoteEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedAcceptancePollVoteEvent.Log.TransactionIndex.Value,
                    TxnHash = castedAcceptancePollVoteEvent.Log.TransactionHash,
                    ThingId = castedAcceptancePollVoteEvent.Event.ThingId,
                    UserId = castedAcceptancePollVoteEvent.Event.UserId.Substring(2).ToLower(),
                    Vote = castedAcceptancePollVoteEvent.Event.Vote,
                    L1BlockNumber = (long)castedAcceptancePollVoteEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<AcceptancePoll.CastedVoteWithReasonEvent> castedAcceptancePollVoteWithReasonEvent)
            {
                yield return new AppEvents.AcceptancePoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedAcceptancePollVoteWithReasonEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedAcceptancePollVoteWithReasonEvent.Log.TransactionIndex.Value,
                    TxnHash = castedAcceptancePollVoteWithReasonEvent.Log.TransactionHash,
                    ThingId = castedAcceptancePollVoteWithReasonEvent.Event.ThingId,
                    UserId = castedAcceptancePollVoteWithReasonEvent.Event.UserId.Substring(2).ToLower(),
                    Vote = castedAcceptancePollVoteWithReasonEvent.Event.Vote,
                    Reason = castedAcceptancePollVoteWithReasonEvent.Event.Reason,
                    L1BlockNumber = (long)castedAcceptancePollVoteWithReasonEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<AcceptancePoll.PollFinalizedEvent> thingAcceptancePollFinalizedEvent)
            {
                yield return new AppEvents.AcceptancePoll.PollFinalized.PollFinalizedEvent
                {
                    BlockNumber = (long)thingAcceptancePollFinalizedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingAcceptancePollFinalizedEvent.Log.TransactionIndex.Value,
                    TxnHash = thingAcceptancePollFinalizedEvent.Log.TransactionHash,
                    ThingId = thingAcceptancePollFinalizedEvent.Event.ThingId,
                    Decision = thingAcceptancePollFinalizedEvent.Event.Decision,
                    VoteAggIpfsCid = thingAcceptancePollFinalizedEvent.Event.VoteAggIpfsCid,
                    RewardedVerifiers = thingAcceptancePollFinalizedEvent.Event.RewardedVerifiers,
                    SlashedVerifiers = thingAcceptancePollFinalizedEvent.Event.SlashedVerifiers
                };
            }
            else if (@event is EventLog<ThingSettlementProposalFundedEvent> thingSettlementProposalFundedEvent)
            {
                yield return new AppEvents.ThingSettlementProposalFunded.ThingSettlementProposalFundedEvent
                {
                    BlockNumber = (long)thingSettlementProposalFundedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingSettlementProposalFundedEvent.Log.TransactionIndex.Value,
                    TxnHash = thingSettlementProposalFundedEvent.Log.TransactionHash,
                    ThingId = thingSettlementProposalFundedEvent.Event.ThingId,
                    SettlementProposalId = thingSettlementProposalFundedEvent.Event.SettlementProposalId,
                    UserId = thingSettlementProposalFundedEvent.Event.UserId.Substring(2).ToLower(),
                    Stake = (decimal)thingSettlementProposalFundedEvent.Event.Stake
                };
            }
            else if (@event is EventLog<ThingAssessmentVerifierLottery.LotteryInitializedEvent> thingAssessmentVerifierLotteryInitializedEvent)
            {
                yield return new AppEvents.ThingAssessmentVerifierLottery.LotteryInitialized.LotteryInitializedEvent
                {
                    BlockNumber = (long)thingAssessmentVerifierLotteryInitializedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingAssessmentVerifierLotteryInitializedEvent.Log.TransactionIndex.Value,
                    TxnHash = thingAssessmentVerifierLotteryInitializedEvent.Log.TransactionHash,
                    ThingId = thingAssessmentVerifierLotteryInitializedEvent.Event.ThingId,
                    SettlementProposalId = thingAssessmentVerifierLotteryInitializedEvent.Event.SettlementProposalId,
                    DataHash = thingAssessmentVerifierLotteryInitializedEvent.Event.DataHash,
                    UserXorDataHash = thingAssessmentVerifierLotteryInitializedEvent.Event.UserXorDataHash,
                };
            }
            else if (@event is EventLog<ThingAssessmentVerifierLottery.ClaimedLotterySpotEvent> thingAssessmentVerifierLotteryClaimedSpotEvent)
            {
                yield return new AppEvents.ThingAssessmentVerifierLottery.ClaimedLotterySpot.ClaimedLotterySpotEvent
                {
                    BlockNumber = (long)thingAssessmentVerifierLotteryClaimedSpotEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingAssessmentVerifierLotteryClaimedSpotEvent.Log.TransactionIndex.Value,
                    TxnHash = thingAssessmentVerifierLotteryClaimedSpotEvent.Log.TransactionHash,
                    ThingId = thingAssessmentVerifierLotteryClaimedSpotEvent.Event.ThingId,
                    SettlementProposalId = thingAssessmentVerifierLotteryClaimedSpotEvent.Event.SettlementProposalId,
                    UserId = thingAssessmentVerifierLotteryClaimedSpotEvent.Event.UserId.Substring(2).ToLower(),
                    L1BlockNumber = (long)thingAssessmentVerifierLotteryClaimedSpotEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<ThingAssessmentVerifierLottery.JoinedLotteryEvent> joinedThingAssessmentVerifierLotteryEvent)
            {
                yield return new AppEvents.ThingAssessmentVerifierLottery.JoinedLottery.JoinedLotteryEvent
                {
                    BlockNumber = (long)joinedThingAssessmentVerifierLotteryEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)joinedThingAssessmentVerifierLotteryEvent.Log.TransactionIndex.Value,
                    TxnHash = joinedThingAssessmentVerifierLotteryEvent.Log.TransactionHash,
                    ThingId = joinedThingAssessmentVerifierLotteryEvent.Event.ThingId,
                    SettlementProposalId = joinedThingAssessmentVerifierLotteryEvent.Event.SettlementProposalId,
                    UserId = joinedThingAssessmentVerifierLotteryEvent.Event.UserId.Substring(2).ToLower(),
                    UserData = joinedThingAssessmentVerifierLotteryEvent.Event.UserData,
                    L1BlockNumber = (long)joinedThingAssessmentVerifierLotteryEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<ThingAssessmentVerifierLottery.LotteryClosedWithSuccessEvent> thingAssessmentVerifierLotteryClosedWithSuccessEvent)
            {
                yield return new AppEvents.ThingAssessmentVerifierLottery.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent
                {
                    BlockNumber = (long)thingAssessmentVerifierLotteryClosedWithSuccessEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)thingAssessmentVerifierLotteryClosedWithSuccessEvent.Log.TransactionIndex.Value,
                    TxnHash = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Log.TransactionHash,
                    ThingId = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.ThingId,
                    SettlementProposalId = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.SettlementProposalId,
                    Orchestrator = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Orchestrator.Substring(2).ToLower(),
                    Data = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Data,
                    UserXorData = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.UserXorData,
                    HashOfL1EndBlock = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.HashOfL1EndBlock,
                    Nonce = (long)thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.Nonce,
                    ClaimantIds = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.ClaimantIds
                        .Select(id => id.Substring(2).ToLower())
                        .ToList(),
                    WinnerIds = thingAssessmentVerifierLotteryClosedWithSuccessEvent.Event.WinnerIds
                        .Select(id => id.Substring(2).ToLower())
                        .ToList()
                };
            }
            else if (@event is EventLog<AssessmentPoll.CastedVoteEvent> castedAssessmentPollVoteEvent)
            {
                yield return new AppEvents.AssessmentPoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedAssessmentPollVoteEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedAssessmentPollVoteEvent.Log.TransactionIndex.Value,
                    TxnHash = castedAssessmentPollVoteEvent.Log.TransactionHash,
                    ThingId = castedAssessmentPollVoteEvent.Event.ThingId,
                    SettlementProposalId = castedAssessmentPollVoteEvent.Event.SettlementProposalId,
                    UserId = castedAssessmentPollVoteEvent.Event.UserId.Substring(2).ToLower(),
                    Vote = castedAssessmentPollVoteEvent.Event.Vote,
                    L1BlockNumber = (long)castedAssessmentPollVoteEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<AssessmentPoll.CastedVoteWithReasonEvent> castedAssessmentPollVoteWithReasonEvent)
            {
                yield return new AppEvents.AssessmentPoll.CastedVote.CastedVoteEvent
                {
                    BlockNumber = (long)castedAssessmentPollVoteWithReasonEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)castedAssessmentPollVoteWithReasonEvent.Log.TransactionIndex.Value,
                    TxnHash = castedAssessmentPollVoteWithReasonEvent.Log.TransactionHash,
                    ThingId = castedAssessmentPollVoteWithReasonEvent.Event.ThingId,
                    SettlementProposalId = castedAssessmentPollVoteWithReasonEvent.Event.SettlementProposalId,
                    UserId = castedAssessmentPollVoteWithReasonEvent.Event.UserId.Substring(2).ToLower(),
                    Vote = castedAssessmentPollVoteWithReasonEvent.Event.Vote,
                    Reason = castedAssessmentPollVoteWithReasonEvent.Event.Reason,
                    L1BlockNumber = (long)castedAssessmentPollVoteWithReasonEvent.Event.L1BlockNumber
                };
            }
            else if (@event is EventLog<AssessmentPoll.PollFinalizedEvent> assessmentPollFinalizedEvent)
            {
                yield return new AppEvents.AssessmentPoll.PollFinalized.PollFinalizedEvent
                {
                    BlockNumber = (long)assessmentPollFinalizedEvent.Log.BlockNumber.Value,
                    TxnIndex = (int)assessmentPollFinalizedEvent.Log.TransactionIndex.Value,
                    TxnHash = assessmentPollFinalizedEvent.Log.TransactionHash,
                    ThingId = assessmentPollFinalizedEvent.Event.ThingId,
                    SettlementProposalId = assessmentPollFinalizedEvent.Event.SettlementProposalId,
                    Decision = assessmentPollFinalizedEvent.Event.Decision,
                    VoteAggIpfsCid = assessmentPollFinalizedEvent.Event.VoteAggIpfsCid,
                    RewardedVerifiers = assessmentPollFinalizedEvent.Event.RewardedVerifiers
                        .Select(id => id.Substring(2).ToLower())
                        .ToList(),
                    SlashedVerifiers = assessmentPollFinalizedEvent.Event.SlashedVerifiers
                        .Select(id => id.Substring(2).ToLower())
                        .ToList()
                };
            }

            tcs.SetResult();
        }
    }
}
