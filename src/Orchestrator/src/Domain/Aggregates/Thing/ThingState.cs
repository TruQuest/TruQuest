namespace Domain.Aggregates;

public enum ThingState
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelectedAndPollInitiated,
    Declined,
    AwaitingSettlement,
    Settled,
}