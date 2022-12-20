using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedVerifierLotteryEventRepository : IRepository<JoinedVerifierLotteryEvent>
{
    void Create(JoinedVerifierLotteryEvent @event);

    Task<List<JoinedVerifierLotteryEvent>> FindWithClosestNonces(
        Guid thingId, long latestBlockNumber, decimal nonce, int count
    );

    Task<List<JoinedVerifierLotteryEvent>> FindWithClosestNoncesAmongUsers(
        Guid thingId, IEnumerable<string> userIds, decimal nonce, int count
    );
}