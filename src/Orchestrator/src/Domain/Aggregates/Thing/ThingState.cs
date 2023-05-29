namespace Domain.Aggregates;

public enum ThingState
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelectedAndPollInitiated,
    ConsensusNotReached,
    Declined,
    AwaitingSettlement,
    Settled,
}