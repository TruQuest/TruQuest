using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedThingSubmissionVerifierLotteryEventRepository : IRepository<JoinedThingSubmissionVerifierLotteryEvent>
{
    void Create(JoinedThingSubmissionVerifierLotteryEvent @event);

    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    );

    Task<List<JoinedThingSubmissionVerifierLotteryEvent>> FindWithClosestNoncesAmongUsers(
        Guid thingId, IEnumerable<string> userIds, decimal nonce, int count
    );
}