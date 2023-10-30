using System.Diagnostics;
using System.Threading.Channels;

using MediatR;

using ThingEvents = Application.Thing.Events;
using ThingEthEvents = Application.Ethereum.Events.ThingValidationVerifierLottery;
using ThingValidationPollEvents = Application.Ethereum.Events.ThingValidationPoll;
using ProposalEvents = Application.Settlement.Events;
using ProposalEthEvents = Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery;
using ProposalAssessmentPollEvents = Application.Ethereum.Events.SettlementProposalAssessmentPoll;
using ThingCommands = Application.Thing.Commands;

namespace Tests.FunctionalTests.Helpers;

public class ThingValidationVerifierLotteryClosedInFailureEventArgs : EventArgs
{
    public required ThingEthEvents.LotteryClosedInFailure.LotteryClosedInFailureEvent Event { get; init; }
}

public class ThingValidationVerifierLotteryClosedWithSuccessEventArgs : EventArgs
{
    public required ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent Event { get; init; }
}

public class SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventArgs : EventArgs
{
    public required ProposalEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent Event { get; init; }
}

public class EventBroadcaster
{
    private readonly ChannelReader<INotification> _eventStream;
    private readonly ChannelReader<IBaseRequest> _requestStream;
    private readonly CancellationTokenSource _cts;

    // @@TODO: This ain't gonna work with multiple flows going at the same time.
    // Use Dictionary? Multiple broadcaster instances?
    public event EventHandler? ThingDraftCreated;
    public event EventHandler? ThingValidationVerifierLotteryInitialized;
    public event EventHandler? JoinedThingValidationVerifierLottery;
    public event EventHandler<ThingValidationVerifierLotteryClosedInFailureEventArgs>? ThingValidationVerifierLotteryClosedInFailure;
    public event EventHandler<ThingValidationVerifierLotteryClosedWithSuccessEventArgs>? ThingValidationVerifierLotteryClosedWithSuccess;
    public event EventHandler? ThingValidationPollFinalized;

    public event EventHandler? ProposalDraftCreated;
    public event EventHandler? ProposalAssessmentVerifierLotteryInitialized;
    public event EventHandler? ClaimedProposalAssessmentVerifierLotterySpot;
    public event EventHandler? JoinedProposalAssessmentVerifierLottery;
    public event EventHandler<SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventArgs>?
        ProposalAssessmentVerifierLotteryClosedWithSuccess;
    public event EventHandler? CastedProposalAssessmentVote;
    public event EventHandler? ProposalAssessmentPollFinalized;

    public event EventHandler? ThingArchived;

    public EventBroadcaster(
        ChannelReader<INotification> eventStream,
        ChannelReader<IBaseRequest> requestStream
    )
    {
        _eventStream = eventStream;
        _requestStream = requestStream;
        _cts = new CancellationTokenSource();
    }

    public async void Start()
    {
        new Thread(_processRequests).Start();

        while (true)
        {
            try
            {
                var @event = await _eventStream.ReadAsync(_cts.Token);
                Debug.WriteLine($"************ {GetType().Name}: {@event.GetType().Name} ************");
                if (@event is ThingEvents.AttachmentsArchivingCompleted.AttachmentsArchivingCompletedEvent)
                {
                    OnThingDraftCreated();
                }
                else if (@event is ThingEthEvents.LotteryInitialized.LotteryInitializedEvent)
                {
                    OnThingValidationVerifierLotteryInitialized();
                }
                else if (@event is ThingEthEvents.JoinedLottery.JoinedLotteryEvent)
                {
                    OnJoinedThingValidationVerifierLottery();
                }
                else if (@event is ThingEthEvents.LotteryClosedInFailure.LotteryClosedInFailureEvent thingLotteryClosedInFailureEvent)
                {
                    OnThingValidationVerifierLotteryClosedInFailure(thingLotteryClosedInFailureEvent);
                }
                else if (@event is ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent thingLotteryClosedWithSuccessEvent)
                {
                    OnThingValidationVerifierLotteryClosedWithSuccess(thingLotteryClosedWithSuccessEvent);
                }
                else if (@event is ThingValidationPollEvents.PollFinalized.PollFinalizedEvent)
                {
                    OnThingValidationPollFinalized();
                }
                else if (@event is ProposalEvents.AttachmentsArchivingCompleted.AttachmentsArchivingCompletedEvent)
                {
                    OnProposalDraftCreated();
                }
                else if (@event is ProposalEthEvents.LotteryInitialized.LotteryInitializedEvent)
                {
                    OnProposalAssessmentVerifierLotteryInitialized();
                }
                else if (@event is ProposalEthEvents.ClaimedLotterySpot.ClaimedLotterySpotEvent)
                {
                    OnClaimedProposalAssessmentVerifierLotterySpot();
                }
                else if (@event is ProposalEthEvents.JoinedLottery.JoinedLotteryEvent)
                {
                    OnJoinedProposalAssessmentVerifierLottery();
                }
                else if (@event is ProposalEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent proposalLotteryClosedWithSuccessEvent)
                {
                    OnProposalAssessmentVerifierLotteryClosedWithSuccess(proposalLotteryClosedWithSuccessEvent);
                }
                else if (@event is ProposalAssessmentPollEvents.CastedVote.CastedVoteEvent)
                {
                    OnCastedProposalAssessmentVote();
                }
                else if (@event is ProposalAssessmentPollEvents.PollFinalized.PollFinalizedEvent)
                {
                    OnProposalAssessmentPollFinalized();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async void _processRequests()
    {
        while (true)
        {
            try
            {
                var request = await _requestStream.ReadAsync(_cts.Token);
                Debug.WriteLine($"************ {GetType().Name}: {request.GetType().Name} ************");
                if (request is ThingCommands.ArchiveDueToFailedLottery.ArchiveDueToFailedLotteryCommand)
                {
                    OnThingArchived();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public void Stop() => _cts.Cancel();

    protected virtual void OnThingDraftCreated() => ThingDraftCreated?.Invoke(this, EventArgs.Empty);

    protected virtual void OnThingValidationVerifierLotteryInitialized() =>
        ThingValidationVerifierLotteryInitialized?.Invoke(this, EventArgs.Empty);

    protected virtual void OnJoinedThingValidationVerifierLottery() =>
        JoinedThingValidationVerifierLottery?.Invoke(this, EventArgs.Empty);

    protected virtual void OnThingValidationVerifierLotteryClosedInFailure(
        ThingEthEvents.LotteryClosedInFailure.LotteryClosedInFailureEvent @event
    ) => ThingValidationVerifierLotteryClosedInFailure?.Invoke(
        this,
        new ThingValidationVerifierLotteryClosedInFailureEventArgs { Event = @event }
    );

    protected virtual void OnThingValidationVerifierLotteryClosedWithSuccess(
        ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent @event
    ) => ThingValidationVerifierLotteryClosedWithSuccess?.Invoke(
        this,
        new ThingValidationVerifierLotteryClosedWithSuccessEventArgs { Event = @event }
    );

    protected virtual void OnThingValidationPollFinalized() =>
        ThingValidationPollFinalized?.Invoke(this, EventArgs.Empty);

    protected virtual void OnProposalDraftCreated() => ProposalDraftCreated?.Invoke(this, EventArgs.Empty);

    protected virtual void OnProposalAssessmentVerifierLotteryInitialized() =>
        ProposalAssessmentVerifierLotteryInitialized?.Invoke(this, EventArgs.Empty);

    protected virtual void OnClaimedProposalAssessmentVerifierLotterySpot() =>
        ClaimedProposalAssessmentVerifierLotterySpot?.Invoke(this, EventArgs.Empty);

    protected virtual void OnJoinedProposalAssessmentVerifierLottery() =>
        JoinedProposalAssessmentVerifierLottery?.Invoke(this, EventArgs.Empty);

    protected virtual void OnProposalAssessmentVerifierLotteryClosedWithSuccess(
        ProposalEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent @event
    ) => ProposalAssessmentVerifierLotteryClosedWithSuccess?.Invoke(
        this,
        new SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEventArgs { Event = @event }
    );

    protected virtual void OnCastedProposalAssessmentVote() =>
        CastedProposalAssessmentVote?.Invoke(this, EventArgs.Empty);

    protected virtual void OnProposalAssessmentPollFinalized() =>
        ProposalAssessmentPollFinalized?.Invoke(this, EventArgs.Empty);

    protected virtual void OnThingArchived() => ThingArchived?.Invoke(this, EventArgs.Empty);
}
