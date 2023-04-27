namespace Domain.Aggregates;

public enum SettlementProposalState
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifiersSelectedAndPollInitiated,
    SoftDeclined,
    HardDeclined,
    Accepted,
}