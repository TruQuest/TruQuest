using Domain.Base;

namespace Domain.Aggregates;

public interface IAssessmentPollVoteRepository : IRepository<AssessmentPollVote>
{
    void Create(AssessmentPollVote vote);
    Task<List<AssessmentPollVote>> GetFor(Guid settlementProposalId);
}