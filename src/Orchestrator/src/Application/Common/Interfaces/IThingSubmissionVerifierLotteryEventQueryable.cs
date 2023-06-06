using Domain.Aggregates.Events;

namespace Application.Common.Interfaces;

public interface IThingSubmissionVerifierLotteryEventQueryable
{
    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> GetJoinedEventsFor(
        Guid thingId, IEnumerable<string> userIds
    );
}