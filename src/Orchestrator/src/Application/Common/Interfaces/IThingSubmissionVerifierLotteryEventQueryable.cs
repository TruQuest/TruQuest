using Domain.Aggregates.Events;

namespace Application.Common.Interfaces;

public interface IThingSubmissionVerifierLotteryEventQueryable
{
    Task<JoinedThingSubmissionVerifierLotteryEvent> GetJoinedEventFor(Guid thingId, string userId);
}
