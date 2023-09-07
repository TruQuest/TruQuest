using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IThingSubmissionVerifierLotteryInitializedEventRepository :
    IRepository<ThingSubmissionVerifierLotteryInitializedEvent>
{
    void Create(ThingSubmissionVerifierLotteryInitializedEvent @event);
}
