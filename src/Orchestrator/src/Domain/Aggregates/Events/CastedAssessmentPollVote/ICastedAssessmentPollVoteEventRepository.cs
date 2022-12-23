using Domain.Base;

namespace Domain.Aggregates.Events;

public interface ICastedAssessmentPollVoteEventRepository : IRepository<CastedAssessmentPollVoteEvent>
{
    void Create(CastedAssessmentPollVoteEvent @event);
    Task<List<CastedAssessmentPollVoteEvent>> GetAllFor(Guid thingId, Guid settlementProposalId);
}