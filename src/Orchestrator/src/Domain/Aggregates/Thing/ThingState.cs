namespace Domain.Aggregates;

public enum ThingState
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

public static class ThingStateExtension
{
    public static string GetString(this ThingState state)
    {
        switch (state)
        {
            case ThingState.Draft:
                return "draft";
            case ThingState.AwaitingFunding:
                return "awaiting_funding";
            case ThingState.FundedAndVerifierLotteryInitiated:
                return "funded_and_verifier_lottery_initiated";
            case ThingState.VerifierLotteryFailed:
                return "verifier_lottery_failed";
            case ThingState.VerifiersSelectedAndPollInitiated:
                return "verifiers_selected_and_poll_initiated";
            case ThingState.ConsensusNotReached:
                return "consensus_not_reached";
            case ThingState.Declined:
                return "declined";
            case ThingState.AwaitingSettlement:
                return "awaiting_settlement";
            case ThingState.Settled:
                return "settled";
        }

        throw new InvalidOperationException();
    }
}
