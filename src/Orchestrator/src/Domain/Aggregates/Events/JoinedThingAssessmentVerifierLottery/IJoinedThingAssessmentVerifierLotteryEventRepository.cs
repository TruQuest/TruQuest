using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedThingAssessmentVerifierLotteryEventRepository : IRepository<JoinedThingAssessmentVerifierLotteryEvent>
{
    void Create(JoinedThingAssessmentVerifierLotteryEvent @event);

    Task<List<JoinedThingAssessmentVerifierLotteryEvent>> FindWithClosestNonces(
        Guid thingId, Guid settlementProposalId, long latestBlockNumber, decimal nonce, int count
    );
}