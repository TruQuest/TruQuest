namespace Application.Settlement.Queries.GetSettlementProposal;

public class SettlementProposalEvidenceQm
{
    public Guid Id { get; }
    public string OriginUrl { get; }
    public string IpfsCid { get; }
    public string PreviewImageIpfsCid { get; }

    public override bool Equals(object? obj)
    {
        var other = obj as SettlementProposalEvidenceQm;
        if (other == null) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
