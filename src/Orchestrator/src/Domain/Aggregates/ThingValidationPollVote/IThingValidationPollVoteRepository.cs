using Domain.Base;

namespace Domain.Aggregates;

public interface IThingValidationPollVoteRepository : IRepository<ThingValidationPollVote>
{
    void Create(ThingValidationPollVote vote);
    Task<List<ThingValidationPollVote>> GetFor(Guid thingId);
}
