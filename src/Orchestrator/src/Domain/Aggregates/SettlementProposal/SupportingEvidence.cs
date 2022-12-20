using Domain.Base;

namespace Domain.Aggregates;

public class SupportingEvidence : Entity
{
    public Guid? Id { get; private set; }
    public string OriginUrl { get; }
    public string TruUrl { get; }

    public SupportingEvidence(string originUrl, string truUrl)
    {
        OriginUrl = originUrl;
        TruUrl = truUrl;
    }
}