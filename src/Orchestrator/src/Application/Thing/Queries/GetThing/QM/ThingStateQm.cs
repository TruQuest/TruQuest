namespace Application.Thing.Queries.GetThing;

public enum ThingStateQm
{
    Draft,
    AwaitingFunding,
    FundedAndSubmissionVerifierLotteryInitiated,
    SubmissionVerifiersSelectedAndPollInitiated,
    SettlementProposalFundedAndAssessmentVerifierLotteryInitiated,
    SettlementProposalAssessmentVerifiersSelectedAndPollInitiated,
}