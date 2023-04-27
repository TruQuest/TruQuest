namespace Domain.Aggregates;

public enum AssessmentDecision
{
    UnsettledDueToInsufficientVotingVolume,
    UnsettledDueToMajorityThresholdNotReached,
    SoftDeclined,
    HardDeclined,
    Accepted
}