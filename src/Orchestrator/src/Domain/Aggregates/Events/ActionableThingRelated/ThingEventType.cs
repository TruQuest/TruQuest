namespace Domain.Aggregates.Events;

public enum ThingEventType
{
    Funded,
    SubmissionVerifierLotteryClosedInFailure,
    SubmissionVerifierLotteryClosedWithSuccess,
    AcceptancePollFinalized,
    SettlementProposalFunded,
    SettlementProposalAssessmentVerifierLotteryClosedInFailure,
    SettlementProposalAssessmentVerifierLotteryClosedWithSuccess,
    SettlementProposalAssessmentPollFinalized,
}
