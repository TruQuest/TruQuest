using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("exportData", typeof(ExportSettlementProposalAssessmentVerifierLotteryDataFunctionOutput))]
public class ExportSettlementProposalAssessmentVerifierLotteryDataMessage : FunctionMessage { }

[FunctionOutput]
public class ExportSettlementProposalAssessmentVerifierLotteryDataFunctionOutput
{
    [Parameter("bytes32[]", "thingProposalIds", 1)]
    public List<byte[]> ThingProposalIds { get; set; }
    [Parameter("tuple[]", "orchestratorCommitments", 2)]
    public List<Commitment> OrchestratorCommitments { get; set; }
    [Parameter("address[][]", "participants", 3)]
    public List<List<string>> Participants { get; set; }
    [Parameter("address[][]", "claimants", 4)]
    public List<List<string>> Claimants { get; set; }
    [Parameter("uint256[][]", "blockNumbers", 5)]
    public List<List<BigInteger>> BlockNumbers { get; set; }
}
