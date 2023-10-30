using Domain.Base;

namespace Domain.Aggregates.Events;

public interface ICastedSettlementProposalAssessmentPollVoteEventRepository : IRepository<CastedSettlementProposalAssessmentPollVoteEvent>
{
    void Create(CastedSettlementProposalAssessmentPollVoteEvent @event);
    Task<List<CastedSettlementProposalAssessmentPollVoteEvent>> GetAllFor(Guid thingId, Guid proposalId);
}
