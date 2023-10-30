using Domain.Base;

namespace Domain.Aggregates;

public class SettlementProposalEvidence : Entity
{
    public Guid? Id { get; private set; }
    public string OriginUrl { get; }
    public string IpfsCid { get; }
    public string PreviewImageIpfsCid { get; }

    public SettlementProposalEvidence(string originUrl, string ipfsCid, string previewImageIpfsCid)
    {
        OriginUrl = originUrl;
        IpfsCid = ipfsCid;
        PreviewImageIpfsCid = previewImageIpfsCid;
    }
}
