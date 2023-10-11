using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingAssessmentPollVoteQueryable
{
    Task<(ThingAssessmentPollResultQm, IEnumerable<VoteQm>)> GetAllFor(Guid proposalId, string? userId);
}
