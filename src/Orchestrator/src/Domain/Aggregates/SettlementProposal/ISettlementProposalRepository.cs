using Domain.Base;

namespace Domain.Aggregates;

public interface ISettlementProposalRepository : IRepository<SettlementProposal>
{
    void Create(SettlementProposal proposal);
    Task<SettlementProposal> FindById(Guid id);
    Task<SettlementProposalState> GetStateFor(Guid proposalId);
    Task UpdateStateFor(Guid proposalId, SettlementProposalState state);
    Task<IReadOnlyList<SettlementProposalVerifier>> GetAllVerifiersFor(Guid settlementProposalId);
    Task<bool> CheckIsDesignatedVerifierFor(Guid proposalId, string userId);
}
