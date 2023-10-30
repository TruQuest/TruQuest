using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingValidationPollVoteQueryable
{
    Task<(ThingValidationPollResultQm, IEnumerable<VoteQm>)> GetAllFor(Guid thingId, string? userId);
}
