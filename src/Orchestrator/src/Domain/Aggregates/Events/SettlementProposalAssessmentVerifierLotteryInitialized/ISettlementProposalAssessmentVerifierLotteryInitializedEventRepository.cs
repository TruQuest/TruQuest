using Domain.Base;

namespace Domain.Aggregates.Events;

public interface ISettlementProposalAssessmentVerifierLotteryInitializedEventRepository :
    IRepository<SettlementProposalAssessmentVerifierLotteryInitializedEvent>
{
    void Create(SettlementProposalAssessmentVerifierLotteryInitializedEvent @event);
}
