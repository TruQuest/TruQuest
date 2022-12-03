namespace Domain.Aggregates;

public enum ThingState
{
    WaitingToBeFunded,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelected
}