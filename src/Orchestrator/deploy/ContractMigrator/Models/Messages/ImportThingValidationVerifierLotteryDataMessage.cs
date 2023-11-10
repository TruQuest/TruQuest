using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

[Function("importData")]
public class ImportThingValidationVerifierLotteryDataMessage : FunctionMessage
{
    [Parameter("bytes16[]", "_thingIds", 1)]
    public List<byte[]> ThingIds { get; set; }
    [Parameter("tuple[]", "_orchestratorCommitments", 2)]
    public List<Commitment> OrchestratorCommitments { get; set; }
    [Parameter("address[][]", "_participants", 3)]
    public List<List<string>> Participants { get; set; }
    [Parameter("uint256[][]", "_blockNumbers", 4)]
    public List<List<BigInteger>> BlockNumbers { get; set; }
}
