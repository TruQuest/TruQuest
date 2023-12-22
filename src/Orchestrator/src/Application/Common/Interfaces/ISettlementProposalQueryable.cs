using Application.General.Queries.GetContractsStates.QM;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Thing.Queries.GetSettlementProposalsList;

namespace Application.Common.Interfaces;

public interface ISettlementProposalQueryable
{
    Task<List<SettlementProposalPreviewQm>> GetForThing(Guid thingId, string? userId);
    Task<SettlementProposalQm?> GetById(Guid id, string? userId);
    Task<List<SettlementProposalTitleAndThingInfoQm>> GetTitleAndThingInfoFor(IEnumerable<Guid> proposalIds);
}
