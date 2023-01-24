namespace Domain.Aggregates;

public enum ThingState
{
    Draft,
    AwaitingFunding,
    FundedAndSubmissionVerifierLotteryInitiated,
    SubmissionVerifiersSelectedAndPollInitiated,
    AwaitingSettlement,
    SettlementProposalFundedAndAssessmentVerifierLotteryInitiated,
    SettlementProposalAssessmentVerifiersSelectedAndPollInitiated,
}