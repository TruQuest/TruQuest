using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

[Function("setSettlementProposalAssessmentVerifierLotteryAddress")]
public class SetSettlementProposalAssessmentVerifierLotteryAddressMessage : FunctionMessage
{
    [Parameter("address", "_settlementProposalAssessmentVerifierLotteryAddress", 1)]
    public string SettlementProposalAssessmentVerifierLotteryAddress { get; init; }
}
