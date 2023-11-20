using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

public class SettlementProposalAssessmentVerifierLotteryDeploymentMessage : ContractDeploymentMessage
{
    public static string Bytecode { get; set; }

    [Parameter("address", "_truQuestAddress", 1)]
    public string TruQuestAddress { get; init; }
    [Parameter("uint8", "_numVerifiers", 2)]
    public int NumVerifiers { get; init; }
    [Parameter("uint16", "_durationBlocks", 3)]
    public int DurationBlocks { get; init; }

    public SettlementProposalAssessmentVerifierLotteryDeploymentMessage() : base(Bytecode) { }
}
