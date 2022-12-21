namespace Domain.Aggregates;

public enum ThingState
{
    AwaitingFunding,
    FundedAndSubmissionVerifierLotteryInitiated,
    SubmissionVerifiersSelectedAndAcceptancePollInitiated,
    AssessmentInProgress,
}