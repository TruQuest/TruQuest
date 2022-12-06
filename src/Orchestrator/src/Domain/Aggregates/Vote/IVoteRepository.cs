using Domain.Base;

namespace Domain.Aggregates;

public interface IVoteRepository : IRepository<Vote>
{
    void Create(Vote vote);
    Task<List<Vote>> GetForThingCastedAt(Guid thingId, long noLaterThanTs, PollType pollType);
}