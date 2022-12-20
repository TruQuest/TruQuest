namespace Domain.Aggregates;

public enum ThingState
{
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelectedAndAcceptancePollInitiated,
    AssessmentInProgress,
}