using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("exportThingSubmitter", typeof(ExportThingSubmitterFunctionOutput))]
public class ExportThingSubmitterMessage : FunctionMessage { }

[FunctionOutput]
public class ExportThingSubmitterFunctionOutput
{
    [Parameter("bytes16[]", "thingIds", 1)]
    public List<byte[]> ThingIds { get; set; }
    [Parameter("address[]", "submitters", 2)]
    public List<string> Submitters { get; set; }
}
