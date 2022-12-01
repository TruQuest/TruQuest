using Domain.Base;
using Domain.QM;

namespace Domain.Aggregates.Events;

public interface IPreJoinedLotteryEventRepository : IRepository<PreJoinedLotteryEvent>
{
    void Create(PreJoinedLotteryEvent @event);
    Task<List<LotteryWinnerQm>> GetLotteryWinnerIndices(Guid thingId, IEnumerable<string> winnerIds);
}