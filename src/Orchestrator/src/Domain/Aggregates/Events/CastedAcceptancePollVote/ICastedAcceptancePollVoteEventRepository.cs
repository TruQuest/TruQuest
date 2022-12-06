using Domain.Base;

namespace Domain.Aggregates.Events;

public interface ICastedAcceptancePollVoteEventRepository : IRepository<CastedAcceptancePollVoteEvent>
{
    void Create(CastedAcceptancePollVoteEvent @event);
    Task<List<CastedAcceptancePollVoteEvent>> GetAllFor(Guid thingId);
}