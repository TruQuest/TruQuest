namespace Domain.Aggregates;

public enum SettlementProposalState
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifierLotteryFailed,
    VerifiersSelectedAndPollInitiated,
    ConsensusNotReached,
    SoftDeclined,
    HardDeclined,
    Accepted,
}