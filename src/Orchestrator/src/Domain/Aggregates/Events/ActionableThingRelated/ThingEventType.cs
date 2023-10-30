namespace Domain.Aggregates.Events;

public enum ThingEventType
{
    Funded,
    ValidationVerifierLotteryClosedInFailure,
    ValidationVerifierLotteryClosedWithSuccess,
    ValidationPollFinalized,
    SettlementProposalFunded,
    SettlementProposalAssessmentVerifierLotteryClosedInFailure,
    SettlementProposalAssessmentVerifierLotteryClosedWithSuccess,
    SettlementProposalAssessmentPollFinalized,
}
