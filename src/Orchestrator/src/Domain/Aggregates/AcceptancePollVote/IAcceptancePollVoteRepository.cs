using Domain.Base;

namespace Domain.Aggregates;

public interface IAcceptancePollVoteRepository : IRepository<AcceptancePollVote>
{
    void Create(AcceptancePollVote vote);
    Task<List<AcceptancePollVote>> GetForThingCastedAt(Guid thingId, long noLaterThanTs);
}