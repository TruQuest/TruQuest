using Domain.Base;

namespace Domain.Aggregates.Events;

public interface ICastedThingValidationPollVoteEventRepository : IRepository<CastedThingValidationPollVoteEvent>
{
    void Create(CastedThingValidationPollVoteEvent @event);
    Task<List<CastedThingValidationPollVoteEvent>> GetAllFor(Guid thingId);
}
