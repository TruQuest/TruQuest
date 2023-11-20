using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

public class TruQuestDeploymentMessage : ContractDeploymentMessage
{
    public static string Bytecode { get; set; }

    [Parameter("address", "_truthserumAddress", 1)]
    public string TruthserumAddress { get; init; }
    [Parameter("uint256", "_verifierStake", 2)]
    public BigInteger VerifierStake { get; init; }
    [Parameter("uint256", "_verifierReward", 3)]
    public BigInteger VerifierReward { get; init; }
    [Parameter("uint256", "_verifierPenalty", 4)]
    public BigInteger VerifierPenalty { get; init; }
    [Parameter("uint256", "_thingStake", 5)]
    public BigInteger ThingStake { get; init; }
    [Parameter("uint256", "_thingAcceptedReward", 6)]
    public BigInteger ThingAcceptedReward { get; init; }
    [Parameter("uint256", "_thingRejectedPenalty", 7)]
    public BigInteger ThingRejectedPenalty { get; init; }
    [Parameter("uint256", "_settlementProposalStake", 8)]
    public BigInteger SettlementProposalStake { get; init; }
    [Parameter("uint256", "_settlementProposalAcceptedReward", 9)]
    public BigInteger SettlementProposalAcceptedReward { get; init; }
    [Parameter("uint256", "_settlementProposalRejectedPenalty", 10)]
    public BigInteger SettlementProposalRejectedPenalty { get; init; }

    public TruQuestDeploymentMessage() : base(Bytecode) { }
}
