using System.Diagnostics;
using System.Threading.Channels;

using MediatR;

using Application.Thing.Events.AttachmentsArchivingCompleted;
using Application.Ethereum.Events.ThingSubmissionVerifierLottery.JoinedLottery;
using Application.Ethereum.Events.ThingSubmissionVerifierLottery.LotteryInitialized;

namespace Tests.FunctionalTests.Helpers;

public class ThingSubmissionVerifierLotteryInitializedEventArgs : EventArgs
{
    public required LotteryInitializedEvent Event { get; init; }
}

public class JoinedThingSubmissionVerifierLotteryEventArgs : EventArgs
{
    public required JoinedLotteryEvent Event { get; init; }
}

public class EventBroadcaster
{
    private readonly ChannelReader<INotification> _stream;
    private readonly CancellationTokenSource _cts;

    public event EventHandler? ThingDraftCreated;
    public event EventHandler<ThingSubmissionVerifierLotteryInitializedEventArgs>? ThingSubmissionVerifierLotteryInitialized;
    public event EventHandler<JoinedThingSubmissionVerifierLotteryEventArgs>? JoinedThingSubmissionVerifierLottery;

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
                else if (@event is LotteryInitializedEvent initEvent)
                {
                    OnThingSubmissionVerifierLotteryInitialized(initEvent);
                }
                else if (@event is JoinedLotteryEvent joinedEvent)
                {
                    OnJoinedThingSubmissionVerifierLottery(joinedEvent);
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

    protected virtual void OnThingSubmissionVerifierLotteryInitialized(LotteryInitializedEvent @event) =>
        ThingSubmissionVerifierLotteryInitialized?.Invoke(
            this,
            new ThingSubmissionVerifierLotteryInitializedEventArgs { Event = @event }
        );

    protected virtual void OnJoinedThingSubmissionVerifierLottery(JoinedLotteryEvent @event) =>
        JoinedThingSubmissionVerifierLottery?.Invoke(
            this,
            new JoinedThingSubmissionVerifierLotteryEventArgs { Event = @event }
        );
}