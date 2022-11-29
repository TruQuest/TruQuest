using Domain.Base;

namespace Domain.Aggregates;

public class Evidence : Entity
{
    public Guid? Id { get; private set; }
    public string OriginUrl { get; }
    public string TruUrl { get; }

    public Evidence(string originUrl, string truUrl)
    {
        OriginUrl = originUrl;
        TruUrl = truUrl;
    }
}