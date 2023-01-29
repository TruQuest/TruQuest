using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAssessmentPollVoteQueryable
{
    Task<IEnumerable<VoteQm>> GetAllFor(Guid proposalId);
}