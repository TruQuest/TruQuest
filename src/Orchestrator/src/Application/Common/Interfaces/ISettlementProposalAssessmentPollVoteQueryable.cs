using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface ISettlementProposalAssessmentPollVoteQueryable
{
    Task<(SettlementProposalAssessmentPollResultQm, IEnumerable<VoteQm>)> GetAllFor(Guid proposalId, string? userId);
}
