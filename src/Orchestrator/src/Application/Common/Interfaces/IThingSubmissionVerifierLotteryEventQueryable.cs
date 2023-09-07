using Domain.Aggregates.Events;

using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingSubmissionVerifierLotteryEventQueryable
{
    Task<JoinedThingSubmissionVerifierLotteryEvent> GetJoinedEventFor(Guid thingId, string userId);
    Task<(
        OrchestratorLotteryCommitmentQm?,
        LotteryClosedEventQm?,
        IEnumerable<VerifierLotteryParticipantEntryQm
    >)> GetOrchestratorCommitmentAndParticipants(Guid thingId);
}
