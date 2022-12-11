namespace Domain.Aggregates;

public enum ThingState
{
    WaitingToBeFunded,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelectedAndAcceptancePollInitiated
}