using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAcceptancePollVoteQueryable
{
    Task<(string?, IEnumerable<VoteQm>)> GetAllFor(Guid thingId, string? userId);
}
