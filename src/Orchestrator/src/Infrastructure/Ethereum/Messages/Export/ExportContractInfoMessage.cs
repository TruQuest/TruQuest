using System.Numerics;

using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Infrastructure.Ethereum.Messages.Export;

[Function("exportContractInfo", typeof(ExportContractInfoFunctionOutput))]
public class ExportContractInfoMessage : FunctionMessage { }

[FunctionOutput]
public class ExportContractInfoFunctionOutput
{
    [Parameter("tuple", "info", 1)]
    public ContractInfo Info { get; set; }
}

public class ContractInfo
{
    [Parameter("string", "version", 1)]
    public string Version { get; set; }
    [Parameter("bool", "stopTheWorld", 2)]
    public bool StopTheWorld { get; set; }
    [Parameter("bool", "withdrawalsEnabled", 3)]
    public bool WithdrawalsEnabled { get; set; }
    [Parameter("address", "truthserumAddress", 4)]
    public string TruthserumAddress { get; set; }
    [Parameter("address", "restrictedAccessAddress", 5)]
    public string RestrictedAccessAddress { get; set; }
    [Parameter("address", "truQuestAddress", 6)]
    public string TruQuestAddress { get; set; }
    [Parameter("address", "thingValidationVerifierLotteryAddress", 7)]
    public string ThingValidationVerifierLotteryAddress { get; set; }
    [Parameter("address", "thingValidationPollAddress", 8)]
    public string ThingValidationPollAddress { get; set; }
    [Parameter("address", "settlementProposalAssessmentVerifierLotteryAddress", 9)]
    public string SettlementProposalAssessmentVerifierLotteryAddress { get; set; }
    [Parameter("address", "settlementProposalAssessmentPollAddress", 10)]
    public string SettlementProposalAssessmentPollAddress { get; set; }
    [Parameter("uint256", "treasury", 11)]
    public BigInteger Treasury { get; set; }
    [Parameter("uint256", "thingStake", 12)]
    public BigInteger ThingStake { get; set; }
    [Parameter("uint256", "verifierStake", 13)]
    public BigInteger VerifierStake { get; set; }
    [Parameter("uint256", "settlementProposalStake", 14)]
    public BigInteger SettlementProposalStake { get; set; }
    [Parameter("uint256", "thingAcceptedReward", 15)]
    public BigInteger ThingAcceptedReward { get; set; }
    [Parameter("uint256", "thingRejectedPenalty", 16)]
    public BigInteger ThingRejectedPenalty { get; set; }
    [Parameter("uint256", "verifierReward", 17)]
    public BigInteger VerifierReward { get; set; }
    [Parameter("uint256", "verifierPenalty", 18)]
    public BigInteger VerifierPenalty { get; set; }
    [Parameter("uint256", "settlementProposalAcceptedReward", 19)]
    public BigInteger SettlementProposalAcceptedReward { get; set; }
    [Parameter("uint256", "settlementProposalRejectedPenalty", 20)]
    public BigInteger SettlementProposalRejectedPenalty { get; set; }
}
