using Domain.Base;

namespace Domain.Aggregates;

public interface ISettlementProposalUpdateRepository : IRepository<SettlementProposalUpdate>
{
    Task AddOrUpdate(SettlementProposalUpdate updateEvent);
}