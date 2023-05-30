namespace Application.Common.Models.QM;

public enum ThingStateQm
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifierLotteryFailed,
    VerifiersSelectedAndPollInitiated,
    ConsensusNotReached,
    Declined,
    AwaitingSettlement,
    Settled,
}