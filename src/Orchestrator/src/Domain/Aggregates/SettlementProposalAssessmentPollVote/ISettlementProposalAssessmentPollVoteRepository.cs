using Domain.Base;

namespace Domain.Aggregates;

public interface ISettlementProposalAssessmentPollVoteRepository : IRepository<SettlementProposalAssessmentPollVote>
{
    void Create(SettlementProposalAssessmentPollVote vote);
    Task<List<SettlementProposalAssessmentPollVote>> GetFor(Guid proposalId);
}
