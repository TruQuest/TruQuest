namespace Domain.Aggregates.Events;

public enum ThingEventType
{
    Funded,
    SubmissionVerifierLotteryClosedInFailure,
    SubmissionVerifierLotteryClosedWithSuccess,
    SettlementProposalFunded,
    SettlementProposalAssessmentVerifierLotteryClosedWithSuccess,
}