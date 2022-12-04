using Domain.Base;

namespace Domain.Aggregates;

public class ThingVerifier : Entity
{
    public Guid? ThingId { get; private set; }
    public string VerifierId { get; }

    internal ThingVerifier(string verifierId)
    {
        VerifierId = verifierId;
    }
}