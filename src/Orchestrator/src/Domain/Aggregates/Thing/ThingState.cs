namespace Domain.Aggregates;

public enum ThingState
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelectedAndPollInitiated,
    AwaitingSettlement,
    Settled,
}