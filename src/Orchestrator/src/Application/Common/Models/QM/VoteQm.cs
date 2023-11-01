namespace Application.Common.Models.QM;

public class VoteQm
{
    public required string UserId { get; init; }
    public required string WalletAddress { get; init; }
    public long? CastedAtMs { get; init; }
    public long? L1BlockNumber { get; init; }
    public long? BlockNumber { get; init; }
    public int? Decision { get; private set; }
    public string? Reason { get; private set; }
    public string? IpfsCid { get; private set; }
    public string? TxnHash { get; private set; }

    public void ClearSensitiveData()
    {
        Decision = null;
        Reason = null;
        IpfsCid = null;
        TxnHash = null;
    }
}
