using Application.Common.Models.QM;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Settlement.Queries.GetSettlementProposals;

namespace Application.Common.Interfaces;

public interface ISettlementProposalQueryable
{
    Task<List<SettlementProposalPreviewQm>> GetAllExceptDraftsOfOthersFor(Guid thingId, string userId);
    Task<List<SettlementProposalPreviewQm>> GetAllExceptDraftsFor(Guid thingId);
    Task<SettlementProposalQm?> GetById(Guid id);
    Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(
        Guid proposalId
    );
    Task<IEnumerable<VerifierQm>> GetVerifiers(Guid proposalId);
}