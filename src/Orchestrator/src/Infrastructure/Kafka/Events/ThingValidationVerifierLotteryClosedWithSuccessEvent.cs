using System.Text;

using KafkaFlow;

using Application.Common.Monitoring;

namespace Infrastructure.Kafka.Events;

internal class ThingValidationVerifierLotteryClosedWithSuccessEvent : TraceableEvent
{
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    public required List<string> WinnerWalletAddresses { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext context)
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key)))
        };
    }
}
