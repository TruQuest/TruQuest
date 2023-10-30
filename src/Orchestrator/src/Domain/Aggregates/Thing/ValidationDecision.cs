namespace Domain.Aggregates;

public enum ValidationDecision
{
    UnsettledDueToInsufficientVotingVolume,
    UnsettledDueToMajorityThresholdNotReached,
    SoftDeclined,
    HardDeclined,
    Accepted
}
