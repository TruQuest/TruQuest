using Domain.Base;
using Domain.QM;

namespace Domain.Aggregates.Events;

public interface IPreJoinedVerifierLotteryEventRepository : IRepository<PreJoinedVerifierLotteryEvent>
{
    void Create(PreJoinedVerifierLotteryEvent @event);
    Task<List<VerifierLotteryWinnerQm>> GetLotteryWinnerIndices(Guid thingId, IEnumerable<string> winnerIds);
}