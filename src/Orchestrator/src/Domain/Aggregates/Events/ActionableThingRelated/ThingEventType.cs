namespace Domain.Aggregates.Events;

public enum ThingEventType
{
    Funded,
    ValidationVerifierLotteryFailed,
    ValidationVerifierLotterySucceeded,
    ValidationPollFinalized,
    SettlementProposalFunded,
    SettlementProposalAssessmentVerifierLotteryFailed,
    SettlementProposalAssessmentVerifierLotterySucceeded,
    SettlementProposalAssessmentPollFinalized,
}

public static class ThingEventTypeExtension
{
    public static string GetString(this ThingEventType thingEventType)
    {
        switch (thingEventType)
        {
            case ThingEventType.Funded:
                return "funded";
            case ThingEventType.ValidationVerifierLotteryFailed:
                return "validation_verifier_lottery_failed";
            case ThingEventType.ValidationVerifierLotterySucceeded:
                return "validation_verifier_lottery_succeeded";
            case ThingEventType.ValidationPollFinalized:
                return "validation_poll_finalized";
            case ThingEventType.SettlementProposalFunded:
                return "settlement_proposal_funded";
            case ThingEventType.SettlementProposalAssessmentVerifierLotteryFailed:
                return "settlement_proposal_assessment_verifier_lottery_failed";
            case ThingEventType.SettlementProposalAssessmentVerifierLotterySucceeded:
                return "settlement_proposal_assessment_verifier_lottery_succeeded";
            case ThingEventType.SettlementProposalAssessmentPollFinalized:
                return "settlement_proposal_assessment_poll_finalized";
        }

        throw new InvalidOperationException();
    }

    public static ThingEventType FromString(string value)
    {
        switch (value)
        {
            case "funded":
                return ThingEventType.Funded;
            case "validation_verifier_lottery_failed":
                return ThingEventType.ValidationVerifierLotteryFailed;
            case "validation_verifier_lottery_succeeded":
                return ThingEventType.ValidationVerifierLotterySucceeded;
            case "validation_poll_finalized":
                return ThingEventType.ValidationPollFinalized;
            case "settlement_proposal_funded":
                return ThingEventType.SettlementProposalFunded;
            case "settlement_proposal_assessment_verifier_lottery_failed":
                return ThingEventType.SettlementProposalAssessmentVerifierLotteryFailed;
            case "settlement_proposal_assessment_verifier_lottery_succeeded":
                return ThingEventType.SettlementProposalAssessmentVerifierLotterySucceeded;
            case "settlement_proposal_assessment_poll_finalized":
                return ThingEventType.SettlementProposalAssessmentPollFinalized;
        }

        throw new InvalidOperationException();
    }
}
