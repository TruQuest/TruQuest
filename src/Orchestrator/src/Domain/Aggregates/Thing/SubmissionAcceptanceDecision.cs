namespace Domain.Aggregates;

public enum SubmissionAcceptanceDecision
{
    UnsettledDueToInsufficientVotingVolume,
    UnsettledDueToMajorityThresholdNotReached,
    SoftDeclined,
    HardDeclined,
    Accepted
}