namespace Domain.Aggregates;

public enum SettlementProposalState
{
    AwaitingFunding,
    FundedAndAssessmentVerifierLotteryInitiated,
    AssessmentVerifiersSelectedAndPollInitiated,
}