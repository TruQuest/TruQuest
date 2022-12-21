using Domain.Base;
using Domain.QM;

namespace Domain.Aggregates.Events;

public interface IPreJoinedThingSubmissionVerifierLotteryEventRepository : IRepository<PreJoinedThingSubmissionVerifierLotteryEvent>
{
    void Create(PreJoinedThingSubmissionVerifierLotteryEvent @event);
    Task<List<VerifierLotteryWinnerQm>> GetLotteryWinnerIndices(Guid thingId, IEnumerable<string> winnerIds);
}