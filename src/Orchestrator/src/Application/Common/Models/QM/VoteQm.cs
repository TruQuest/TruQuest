namespace Application.Common.Models.QM;

public class VoteQm
{
    public string UserId { get; init; }
    public long? CastedAtMs { get; init; }
    public long? BlockNumber { get; init; }
}