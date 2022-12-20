using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Events;

[Event("ThingSettlementProposalFunded")]
public class ThingSettlementProposalFundedEvent : IEventDTO
{
    [Parameter("string", "thingId", 1, true)]
    public string ThingIdHash { get; set; }
    [Parameter("string", "settlementProposalId", 2, true)]
    public string SettlementProposalIdHash { get; set; }
    [Parameter("address", "user", 3, true)]
    public string UserId { get; set; }
    [Parameter("uint256", "thingSettlementProposalStake", 4, false)]
    public BigInteger Stake { get; set; }
}