namespace Application.General.Queries.GetContractsStates.QM;

public class TruQuestContractInfoQm
{
    public required string Version { get; init; }
    public required bool StopTheWorld { get; init; }
    public required bool WithdrawalsEnabled { get; init; }
    public required string TruthserumAddress { get; init; }
    public required string RestrictedAccessAddress { get; init; }
    public required string TruQuestAddress { get; init; }
    public required string ThingValidationVerifierLotteryAddress { get; init; }
    public required string ThingValidationPollAddress { get; init; }
    public required string SettlementProposalAssessmentVerifierLotteryAddress { get; init; }
    public required string SettlementProposalAssessmentPollAddress { get; init; }
    public required string HexTreasury { get; init; }
    public required string HexThingStake { get; init; }
    public required string HexVerifierStake { get; init; }
    public required string HexSettlementProposalStake { get; init; }
    public required string HexThingAcceptedReward { get; init; }
    public required string HexThingRejectedPenalty { get; init; }
    public required string HexVerifierReward { get; init; }
    public required string HexVerifierPenalty { get; init; }
    public required string HexSettlementProposalAcceptedReward { get; init; }
    public required string HexSettlementProposalRejectedPenalty { get; init; }
}
