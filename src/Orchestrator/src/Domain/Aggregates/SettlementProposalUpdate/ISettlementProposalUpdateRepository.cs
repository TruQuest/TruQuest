using Domain.Base;

namespace Domain.Aggregates;

public interface ISettlementProposalUpdateRepository : IRepository<SettlementProposalUpdate>
{
    Task AddOrUpdate(params SettlementProposalUpdate[] updateEvents);
}