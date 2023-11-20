using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setPolls")]
public class SetPollsMessage : FunctionMessage
{
    [Parameter("address", "_thingValidationPollAddress", 1)]
    public string ThingValidationPollAddress { get; init; }
    [Parameter("address", "_settlementProposalAssessmentPollAddress", 2)]
    public string SettlementProposalAssessmentPollAddress { get; init; }
}
