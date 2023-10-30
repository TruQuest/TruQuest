using Domain.Base;

namespace Domain.Aggregates.Events;

public interface IJoinedSettlementProposalAssessmentVerifierLotteryEventRepository :
    IRepository<JoinedSettlementProposalAssessmentVerifierLotteryEvent>
{
    void Create(JoinedSettlementProposalAssessmentVerifierLotteryEvent @event);
    Task<List<JoinedSettlementProposalAssessmentVerifierLotteryEvent>> FindAllFor(Guid thingId, Guid proposalId);
    Task UpdateNoncesFor(IEnumerable<JoinedSettlementProposalAssessmentVerifierLotteryEvent> events);
}
