using System.Numerics;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("exportData", typeof(ExportThingValidationPollDataFunctionOutput))]
public class ExportThingValidationPollDataMessage : FunctionMessage { }

[FunctionOutput]
public class ExportThingValidationPollDataFunctionOutput
{
    [Parameter("bytes16[]", "thingIds", 1)]
    public List<byte[]> ThingIds { get; set; }
    [Parameter("int256[]", "initBlockNumbers", 2)]
    public List<BigInteger> InitBlockNumbers { get; set; }
    [Parameter("address[][]", "verifiers", 3)]
    public List<List<string>> Verifiers { get; set; }
}
