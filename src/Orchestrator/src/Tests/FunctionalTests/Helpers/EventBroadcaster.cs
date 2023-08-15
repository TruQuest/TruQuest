using System.Diagnostics;
using System.Threading.Channels;

using MediatR;

using ThingEvents = Application.Thing.Events;
using ThingEthEvents = Application.Ethereum.Events.ThingSubmissionVerifierLottery;
using AcceptancePollEvents = Application.Ethereum.Events.AcceptancePoll;
using ProposalEvents = Application.Settlement.Events;
using ProposalEthEvents = Application.Ethereum.Events.ThingAssessmentVerifierLottery;
using AssessmentPollEvents = Application.Ethereum.Events.AssessmentPoll;
using ThingCommands = Application.Thing.Commands;

namespace Tests.FunctionalTests.Helpers;

public class ThingSubmissionVerifierLotteryClosedInFailureEventArgs : EventArgs
{
    public required ThingEthEvents.LotteryClosedInFailure.LotteryClosedInFailureEvent Event { get; init; }
}

public class ThingSubmissionVerifierLotteryClosedWithSuccessEventArgs : EventArgs
{
    public required ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent Event { get; init; }
}

public class ProposalAssessmentVerifierLotteryClosedWithSuccessEventArgs : EventArgs
{
    public required ProposalEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent Event { get; init; }
}

public class EventBroadcaster
{
    private readonly ChannelReader<INotification> _eventStream;
    private readonly ChannelReader<IBaseRequest> _requestStream;
    private readonly CancellationTokenSource _cts;

    // @@TODO: This ain't gonna work with multiple flows going at the same time.
    // Use Dictionary?
    public event EventHandler? ThingDraftCreated;
    public event EventHandler? ThingSubmissionVerifierLotteryInitialized;
    public event EventHandler? JoinedThingSubmissionVerifierLottery;
    public event EventHandler<ThingSubmissionVerifierLotteryClosedInFailureEventArgs>? ThingSubmissionVerifierLotteryClosedInFailure;
    public event EventHandler<ThingSubmissionVerifierLotteryClosedWithSuccessEventArgs>? ThingSubmissionVerifierLotteryClosedWithSuccess;
    public event EventHandler? ThingAcceptancePollFinalized;

    public event EventHandler? ProposalDraftCreated;
    public event EventHandler? ProposalAssessmentVerifierLotteryInitialized;
    public event EventHandler? ClaimedProposalAssessmentVerifierLotterySpot;
    public event EventHandler? JoinedProposalAssessmentVerifierLottery;
    public event EventHandler<ProposalAssessmentVerifierLotteryClosedWithSuccessEventArgs>?
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
                    OnThingSubmissionVerifierLotteryInitialized();
                }
                else if (@event is ThingEthEvents.JoinedLottery.JoinedLotteryEvent)
                {
                    OnJoinedThingSubmissionVerifierLottery();
                }
                else if (@event is ThingEthEvents.LotteryClosedInFailure.LotteryClosedInFailureEvent thingLotteryClosedInFailureEvent)
                {
                    OnThingSubmissionVerifierLotteryClosedInFailure(thingLotteryClosedInFailureEvent);
                }
                else if (@event is ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent thingLotteryClosedWithSuccessEvent)
                {
                    OnThingSubmissionVerifierLotteryClosedWithSuccess(thingLotteryClosedWithSuccessEvent);
                }
                else if (@event is AcceptancePollEvents.PollFinalized.PollFinalizedEvent)
                {
                    OnThingAcceptancePollFinalized();
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
                else if (@event is AssessmentPollEvents.CastedVote.CastedVoteEvent)
                {
                    OnCastedProposalAssessmentVote();
                }
                else if (@event is AssessmentPollEvents.PollFinalized.PollFinalizedEvent)
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

    protected virtual void OnThingSubmissionVerifierLotteryInitialized() =>
        ThingSubmissionVerifierLotteryInitialized?.Invoke(this, EventArgs.Empty);

    protected virtual void OnJoinedThingSubmissionVerifierLottery() =>
        JoinedThingSubmissionVerifierLottery?.Invoke(this, EventArgs.Empty);

    protected virtual void OnThingSubmissionVerifierLotteryClosedInFailure(
        ThingEthEvents.LotteryClosedInFailure.LotteryClosedInFailureEvent @event
    ) => ThingSubmissionVerifierLotteryClosedInFailure?.Invoke(
        this,
        new ThingSubmissionVerifierLotteryClosedInFailureEventArgs { Event = @event }
    );

    protected virtual void OnThingSubmissionVerifierLotteryClosedWithSuccess(
        ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent @event
    ) => ThingSubmissionVerifierLotteryClosedWithSuccess?.Invoke(
        this,
        new ThingSubmissionVerifierLotteryClosedWithSuccessEventArgs { Event = @event }
    );

    protected virtual void OnThingAcceptancePollFinalized() =>
        ThingAcceptancePollFinalized?.Invoke(this, EventArgs.Empty);

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
        new ProposalAssessmentVerifierLotteryClosedWithSuccessEventArgs { Event = @event }
    );

    protected virtual void OnCastedProposalAssessmentVote() =>
        CastedProposalAssessmentVote?.Invoke(this, EventArgs.Empty);

    protected virtual void OnProposalAssessmentPollFinalized() =>
        ProposalAssessmentPollFinalized?.Invoke(this, EventArgs.Empty);

    protected virtual void OnThingArchived() => ThingArchived?.Invoke(this, EventArgs.Empty);
}
