using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IClaimedSettlementProposalAssessmentVerifierLotterySpotEventRepository :
    IRepository<ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent>
{
    void Create(ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent @event);
    Task<List<ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent>> FindAllFor(
        Guid thingId, Guid proposalId
    );
    Task UpdateNoncesFor(IEnumerable<ClaimedSettlementProposalAssessmentVerifierLotterySpotEvent> events);
}
