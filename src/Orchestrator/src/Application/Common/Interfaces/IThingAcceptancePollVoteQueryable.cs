using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAcceptancePollVoteQueryable
{
    Task<(string?, IEnumerable<Vote2Qm>)> GetAllFor(Guid thingId, string? userId);
}
