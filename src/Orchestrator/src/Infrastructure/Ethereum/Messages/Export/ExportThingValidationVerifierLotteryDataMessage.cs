using System.Numerics;

using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("exportData", typeof(ExportThingValidationVerifierLotteryDataFunctionOutput))]
public class ExportThingValidationVerifierLotteryDataMessage : FunctionMessage { }

[FunctionOutput]
public class ExportThingValidationVerifierLotteryDataFunctionOutput
{
    [Parameter("bytes16[]", "thingIds", 1)]
    public List<byte[]> ThingIds { get; set; }
    [Parameter("tuple[]", "orchestratorCommitments", 2)]
    public List<Commitment> OrchestratorCommitments { get; set; }
    [Parameter("address[][]", "participants", 3)]
    public List<List<string>> Participants { get; set; }
    [Parameter("uint256[][]", "blockNumbers", 4)]
    public List<List<BigInteger>> BlockNumbers { get; set; }
}

public class Commitment
{
    [Parameter("bytes32", "dataHash", 1)]
    public byte[] DataHash { get; set; }
    [Parameter("bytes32", "userXorDataHash", 2)]
    public byte[] UserXorDataHash { get; set; }
    [Parameter("int256", "block", 3)]
    public BigInteger Block { get; set; }
}
