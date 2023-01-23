using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IPreJoinedThingSubmissionVerifierLotteryEventRepository :
    IRepository<PreJoinedThingSubmissionVerifierLotteryEvent>
{
    void Create(PreJoinedThingSubmissionVerifierLotteryEvent @event);
}