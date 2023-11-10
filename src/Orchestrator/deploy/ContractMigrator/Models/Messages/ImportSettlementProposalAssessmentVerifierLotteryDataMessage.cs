using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

[Function("importData")]
public class ImportSettlementProposalAssessmentVerifierLotteryDataMessage : FunctionMessage
{
    [Parameter("bytes32[]", "_thingProposalIds", 1)]
    public List<byte[]> ThingProposalIds { get; set; }
    [Parameter("tuple[]", "_orchestratorCommitments", 2)]
    public List<Commitment> OrchestratorCommitments { get; set; }
    [Parameter("address[][]", "_participants", 3)]
    public List<List<string>> Participants { get; set; }
    [Parameter("address[][]", "_claimants", 4)]
    public List<List<string>> Claimants { get; set; }
    [Parameter("uint256[][]", "_blockNumbers", 5)]
    public List<List<BigInteger>> BlockNumbers { get; set; }
}
