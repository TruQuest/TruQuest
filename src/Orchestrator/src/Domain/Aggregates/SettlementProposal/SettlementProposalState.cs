namespace Domain.Aggregates;

public enum SettlementProposalState
{
    Draft,
    AwaitingFunding,
    FundedAndAssessmentVerifierLotteryInitiated,
    AssessmentVerifiersSelectedAndPollInitiated,
}