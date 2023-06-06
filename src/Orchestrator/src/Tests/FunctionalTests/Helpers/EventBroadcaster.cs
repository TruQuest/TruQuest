using System.Diagnostics;
using System.Threading.Channels;

using MediatR;

using Application.Thing.Events.AttachmentsArchivingCompleted;
using Application.Ethereum.Events.ThingSubmissionVerifierLottery.JoinedLottery;
using Application.Ethereum.Events.ThingSubmissionVerifierLottery.LotteryInitialized;
using Application.Ethereum.Events.ThingSubmissionVerifierLottery.LotteryClosedWithSuccess;
using Application.Ethereum.Events.AcceptancePoll.PollFinalized;

namespace Tests.FunctionalTests.Helpers;

public class ThingSubmissionVerifierLotteryClosedWithSuccessEventArgs : EventArgs
{
    public required LotteryClosedWithSuccessEvent Event { get; init; }
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
                if (@event is AttachmentsArchivingCompletedEvent)
                {
                    OnThingDraftCreated();
                }
                else if (@event is LotteryInitializedEvent)
                {
                    OnThingSubmissionVerifierLotteryInitialized();
                }
                else if (@event is JoinedLotteryEvent)
                {
                    OnJoinedThingSubmissionVerifierLottery();
                }
                else if (@event is LotteryClosedWithSuccessEvent lotteryClosedWithSuccessEvent)
                {
                    OnThingSubmissionVerifierLotteryClosedWithSuccess(lotteryClosedWithSuccessEvent);
                }
                else if (@event is PollFinalizedEvent)
                {
                    OnThingAcceptancePollFinalized();
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

    protected virtual void OnThingSubmissionVerifierLotteryClosedWithSuccess(LotteryClosedWithSuccessEvent @event) =>
        ThingSubmissionVerifierLotteryClosedWithSuccess?.Invoke(
            this,
            new ThingSubmissionVerifierLotteryClosedWithSuccessEventArgs { Event = @event }
        );

    protected virtual void OnThingAcceptancePollFinalized() =>
        ThingAcceptancePollFinalized?.Invoke(this, EventArgs.Empty);
}