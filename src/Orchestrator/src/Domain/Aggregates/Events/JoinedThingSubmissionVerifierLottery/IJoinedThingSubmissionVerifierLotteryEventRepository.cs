using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedThingSubmissionVerifierLotteryEventRepository :
    IRepository<JoinedThingSubmissionVerifierLotteryEvent>
{
    void Create(JoinedThingSubmissionVerifierLotteryEvent @event);
}