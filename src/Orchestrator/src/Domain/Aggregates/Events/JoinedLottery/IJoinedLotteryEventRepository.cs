using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedLotteryEventRepository : IRepository<JoinedLotteryEvent>
{
    void Create(JoinedLotteryEvent @event);

    Task<List<JoinedLotteryEvent>> FindWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    );
}