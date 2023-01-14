namespace Domain.Aggregates;

public enum ThingState
{
    Draft,
    AwaitingFunding,
    FundedAndSubmissionVerifierLotteryInitiated,
    SubmissionVerifiersSelectedAndPollInitiated,
    SettlementProposalFundedAndAssessmentVerifierLotteryInitiated,
    SettlementProposalAssessmentVerifiersSelectedAndPollInitiated,
}