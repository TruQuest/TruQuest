using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setLotteryAddresses")]
public class SetLotteryAddressesMessage : FunctionMessage
{
    [Parameter("address", "_thingValidationVerifierLotteryAddress", 1)]
    public string ThingValidationVerifierLotteryAddress { get; init; }
    [Parameter("address", "_settlementProposalAssessmentVerifierLotteryAddress", 2)]
    public string SettlementProposalAssessmentVerifierLotteryAddress { get; init; }
}
