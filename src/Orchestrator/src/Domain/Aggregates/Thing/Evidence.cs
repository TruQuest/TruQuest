using Domain.Base;

namespace Domain.Aggregates;

public class Evidence : Entity
{
    public Guid? Id { get; private set; }
    public string OriginURL { get; }
    public string TruURL { get; }

    public Evidence(string originURL, string truURL)
    {
        OriginURL = originURL;
        TruURL = truURL;
    }
}