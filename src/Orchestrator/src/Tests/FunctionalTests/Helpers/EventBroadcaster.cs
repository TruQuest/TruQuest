using System.Diagnostics;
using System.Threading.Channels;

using MediatR;

using ThingEvents = Application.Thing.Events;
using ThingEthEvents = Application.Ethereum.Events.ThingSubmissionVerifierLottery;
using AcceptancePollEvents = Application.Ethereum.Events.AcceptancePoll;
using ProposalEvents = Application.Settlement.Events;
using ProposalEthEvents = Application.Ethereum.Events.ThingAssessmentVerifierLottery;
using AssessmentPollEvents = Application.Ethereum.Events.AssessmentPoll;

namespace Tests.FunctionalTests.Helpers;

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
    private readonly ChannelReader<INotification> _stream;
    private readonly CancellationTokenSource _cts;

    public event EventHandler? ThingDraftCreated;
    public event EventHandler? ThingSubmissionVerifierLotteryInitialized;
    public event EventHandler? JoinedThingSubmissionVerifierLottery;
    public event EventHandler<ThingSubmissionVerifierLotteryClosedWithSuccessEventArgs>? ThingSubmissionVerifierLotteryClosedWithSuccess;
    public event EventHandler? ThingAcceptancePollFinalized;

    public event EventHandler? ProposalDraftCreated;
    public event EventHandler? ProposalAssessmentVerifierLotteryInitialized;
    public event EventHandler? ClaimedProposalAssessmentVerifierLotterySpot;
    public event EventHandler? JoinedProposalAssessmentVerifierLottery;
    public event EventHandler<ProposalAssessmentVerifierLotteryClosedWithSuccessEventArgs>? ProposalAssessmentVerifierLotteryClosedWithSuccess;
    public event EventHandler? CastedProposalAssessmentVote;
    public event EventHandler? ProposalAssessmentPollFinalized;

    public EventBroadcaster(ChannelReader<INotification> stream)
    {
        _stream = stream;
        _cts = new CancellationTokenSource();
    }

    public async void Start()
    {
        while (true)
        {
            try
            {
                var @event = await _stream.ReadAsync(_cts.Token);
                Debug.WriteLine($"{GetType().Name}: {@event.GetType().Name}");
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

    public void Stop() => _cts.Cancel();

    protected virtual void OnThingDraftCreated() => ThingDraftCreated?.Invoke(this, EventArgs.Empty);

    protected virtual void OnThingSubmissionVerifierLotteryInitialized() =>
        ThingSubmissionVerifierLotteryInitialized?.Invoke(this, EventArgs.Empty);

    protected virtual void OnJoinedThingSubmissionVerifierLottery() =>
        JoinedThingSubmissionVerifierLottery?.Invoke(this, EventArgs.Empty);

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
}