namespace Domain.Aggregates;

public enum ThingState
{
    WaitingToBeFunded,
    Funded,
    VerifierLotteryInProgress,
    VerifiersSelected
}