namespace Infrastructure.Ethereum.Messages;

internal enum Decision
{
    UnsettledDueToInsufficientVotingVolume,
    UnsettledDueToMajorityThresholdNotReached,
    SoftDeclined,
    HardDeclined,
    Accepted
}