namespace Application.Thing.Queries.GetThing;

public enum ThingStateQm
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelectedAndPollInitiated,
    AwaitingSettlement,
    Settled,
}