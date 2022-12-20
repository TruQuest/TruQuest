using Domain.Base;
using Domain.QM;

namespace Domain.Aggregates.Events;

public interface IPreJoinedThingAssessmentVerifierLotteryEventRepository : IRepository<PreJoinedThingAssessmentVerifierLotteryEvent>
{
    void Create(PreJoinedThingAssessmentVerifierLotteryEvent @event);

    Task<List<VerifierLotteryWinnerQm>> GetLotteryWinnerIndices(
        Guid thingId, Guid settlementProposalId, IEnumerable<string> winnerIds
    );
}