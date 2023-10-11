namespace Application.Common.Models.QM;

public enum SettlementProposalStateQm
{
    Draft,
    AwaitingFunding,
    FundedAndVerifierLotteryInitiated,
    VerifierLotteryFailed,
    VerifiersSelectedAndPollInitiated,
    ConsensusNotReached,
    Declined,
    Accepted,
}
