namespace Domain.Aggregates;

public enum SubmissionEvaluationDecision
{
    UnsettledDueToInsufficientVotingVolume,
    UnsettledDueToMajorityThresholdNotReached,
    SoftDeclined,
    HardDeclined,
    Accepted
}