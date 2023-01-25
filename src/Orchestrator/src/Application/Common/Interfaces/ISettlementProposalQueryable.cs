using Application.Settlement.Queries.GetSettlementProposals;

namespace Application.Common.Interfaces;

public interface ISettlementProposalQueryable
{
    Task<List<SettlementProposalPreviewQm>> GetAllExceptDraftsOfOthersFor(Guid thingId, string userId);
    Task<List<SettlementProposalPreviewQm>> GetAllExceptDraftsFor(Guid thingId);
}