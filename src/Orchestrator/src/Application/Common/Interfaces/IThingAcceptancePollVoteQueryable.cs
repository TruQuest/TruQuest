using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAcceptancePollVoteQueryable
{
    Task<(ThingAcceptancePollResultQm, IEnumerable<VoteQm>)> GetAllFor(Guid thingId, string? userId);
}
