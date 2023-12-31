using System.Text;

using KafkaFlow;

using Application.Common.Monitoring;

namespace Infrastructure.Kafka.Events;

internal class SettlementProposalAssessmentVerifierLotteryClosedWithSuccessEvent : TraceableEvent
{
    public required Guid SettlementProposalId { get; init; }
    public required string Orchestrator { get; init; }
    public required string Data { get; init; }
    public required string UserXorData { get; init; }
    public required string HashOfL1EndBlock { get; init; }
    public required long Nonce { get; init; }
    // @@BUG: For some reason when claimants are empty debezium discards
    // the key-value pair entirely, so if this property is set to 'required'
    // json deserialization fails.
    public List<string> ClaimantWalletAddresses { get; init; } = new();
    public required List<string> WinnerWalletAddresses { get; init; }

    public override IEnumerable<(string Name, object? Value)> GetActivityTags(IMessageContext context)
    {
        return new (string Name, object? Value)[]
        {
            (ActivityTags.ThingId, Guid.Parse(Encoding.UTF8.GetString((byte[])context.Message.Key))),
            (ActivityTags.SettlementProposalId, SettlementProposalId)
        };
    }
}
