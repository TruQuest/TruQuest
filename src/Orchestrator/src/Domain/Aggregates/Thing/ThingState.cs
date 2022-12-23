namespace Domain.Aggregates;

public enum ThingState
{
    AwaitingFunding,
    FundedAndSubmissionVerifierLotteryInitiated,
    SubmissionVerifiersSelectedAndPollInitiated,
    SettlementProposalFundedAndAssessmentVerifierLotteryInitiated,
    SettlementProposalAssessmentVerifiersSelectedAndPollInitiated,
}