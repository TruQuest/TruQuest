using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("exportData", typeof(ExportSettlementProposalAssessmentPollDataFunctionOutput))]
public class ExportSettlementProposalAssessmentPollDataMessage : FunctionMessage { }

[FunctionOutput]
public class ExportSettlementProposalAssessmentPollDataFunctionOutput
{
    [Parameter("bytes32[]", "thingProposalIds", 1)]
    public List<byte[]> ThingProposalIds { get; set; }
    [Parameter("int256[]", "initBlockNumbers", 2)]
    public List<BigInteger> InitBlockNumbers { get; set; }
    [Parameter("address[][]", "verifiers", 3)]
    public List<List<string>> Verifiers { get; set; }
}
