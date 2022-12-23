using Domain.Base;

namespace Domain.Aggregates;

public class SettlementProposalVerifier : Entity
{
    public Guid? SettlementProposalId { get; private set; }
    public string VerifierId { get; }

    public SettlementProposalVerifier(string verifierId)
    {
        VerifierId = verifierId;
    }
}