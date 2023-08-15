using Domain.Aggregates;

namespace Application.Common.Models.QM;

public class VoteQm
{
    public string UserId { get; init; }
    public long? CastedAtMs { get; init; }
    public long? BlockNumber { get; init; }
}

public class Vote2Qm
{
    public string UserId { get; init; }
    public long? CastedAtMs { get; init; }
    public long? L1BlockNumber { get; init; }
    public long? BlockNumber { get; init; }
    public AcceptancePollVote.VoteDecision? Decision { get; private set; }
    public string? Reason { get; private set; }
    public string? IpfsCid { get; private set; }
    public string? TxnHash { get; private set; }

    public void ClearSensitiveInfo()
    {
        Decision = null;
        Reason = null;
        IpfsCid = null;
        TxnHash = null;
    }
}
