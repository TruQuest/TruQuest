using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setLotteryAndPollAddresses")]
public class SetLotteryAndPollAddressesMessage : FunctionMessage
{
    [Parameter("address", "_thingValidationVerifierLotteryAddress", 1)]
    public string ThingValidationVerifierLotteryAddress { get; init; }
    [Parameter("address", "_thingValidationPollAddress", 2)]
    public string ThingValidationPollAddress { get; init; }
    [Parameter("address", "_settlementProposalAssessmentVerifierLotteryAddress", 3)]
    public string SettlementProposalAssessmentVerifierLotteryAddress { get; init; }
    [Parameter("address", "_settlementProposalAssessmentPollAddress", 4)]
    public string SettlementProposalAssessmentPollAddress { get; init; }
}
