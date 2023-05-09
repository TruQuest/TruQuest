using Application.Common.Models.QM;
using Application.Settlement.Queries.GetSettlementProposal;
using Application.Thing.Queries.GetSettlementProposalsList;

namespace Application.Common.Interfaces;

public interface ISettlementProposalQueryable
{
    Task<List<SettlementProposalPreviewQm>> GetForThing(Guid thingId, string? userId);
    Task<SettlementProposalQm?> GetById(Guid id);
    Task<IEnumerable<VerifierLotteryParticipantEntryQm>> GetVerifierLotteryParticipants(
        Guid proposalId
    );
    Task<IEnumerable<VerifierQm>> GetVerifiers(Guid proposalId);
}