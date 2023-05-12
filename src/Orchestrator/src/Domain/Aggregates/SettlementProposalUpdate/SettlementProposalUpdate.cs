using Domain.Base;

namespace Domain.Aggregates;

public class SettlementProposalUpdate : Entity, IAggregateRoot
{
    public Guid SettlementProposalId { get; }
    public SettlementProposalUpdateCategory Category { get; }
    public long UpdateTimestamp { get; }
    public string Title { get; }
    public string? Details { get; }

    public SettlementProposalUpdate(
        Guid settlementProposalId, SettlementProposalUpdateCategory category,
        long updateTimestamp, string title, string? details = null
    )
    {
        SettlementProposalId = settlementProposalId;
        Category = category;
        UpdateTimestamp = updateTimestamp;
        Title = title;
        Details = details;
    }
}