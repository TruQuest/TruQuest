using Application.General.Queries.GetContractsStates.QM;

namespace Application.General.Queries.GetContractsStates;

public class GetContractsStatesRvm
{
    public required IEnumerable<string> WhitelistedUsers { get; init; }
    public required IEnumerable<UserBalanceQm> UserBalances { get; init; }
    public required IEnumerable<ThingSubmitterQm> ThingSubmitters { get; init; }
    public required IEnumerable<SettlementProposalSubmitterQm> SettlementProposalSubmitters { get; init; }
    public required IEnumerable<ThingValidationVerifierLotteryQm> ThingValidationVerifierLotteries { get; init; }
    public required IEnumerable<ThingValidationPollQm> ThingValidationPolls { get; init; }
    public required IEnumerable<SettlementProposalAssessmentVerifierLotteryQm> SettlementProposalAssessmentVerifierLotteries { get; init; }
    public required IEnumerable<SettlementProposalAssessmentPollQm> SettlementProposalAssessmentPolls { get; init; }
}
