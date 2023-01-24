using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAcceptancePollVoteQueryable
{
    Task<IEnumerable<VoteQm>> GetAllFor(Guid thingId);
}