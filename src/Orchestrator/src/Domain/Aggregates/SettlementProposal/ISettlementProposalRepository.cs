using Domain.Base;

namespace Domain.Aggregates;

public interface ISettlementProposalRepository : IRepository<SettlementProposal>
{
    void Create(SettlementProposal proposal);
    Task<SettlementProposal> FindByIdHash(string idHash);
}