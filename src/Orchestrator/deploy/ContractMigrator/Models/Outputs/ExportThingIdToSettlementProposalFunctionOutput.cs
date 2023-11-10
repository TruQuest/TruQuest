using Nethereum.ABI.FunctionEncoding.Attributes;

[FunctionOutput]
public class ExportThingIdToSettlementProposalFunctionOutput
{
    [Parameter("bytes16[]", "thingIds", 1)]
    public List<byte[]> ThingIds { get; set; }
    [Parameter("tuple[]", "settlementProposals", 2)]
    public List<SettlementProposal> SettlementProposals { get; set; }
}

public class SettlementProposal
{
    [Parameter("bytes16", "id", 1)]
    public byte[] Id { get; set; }
    [Parameter("address", "submitter", 2)]
    public string Submitter { get; set; }
}
