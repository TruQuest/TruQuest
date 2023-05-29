namespace Application.Common.Models.QM;

public enum ThingStateQm
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