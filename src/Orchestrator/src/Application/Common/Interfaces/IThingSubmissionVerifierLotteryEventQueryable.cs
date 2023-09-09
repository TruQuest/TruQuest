using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingSubmissionVerifierLotteryEventQueryable
{
    Task<string> GetJoinedEventUserDataFor(Guid thingId, string userId);
    Task<(
        OrchestratorLotteryCommitmentQm?,
        LotteryClosedEventQm?,
        IEnumerable<VerifierLotteryParticipantEntryQm>
    )> GetOrchestratorCommitmentAndParticipants(Guid thingId);
}
