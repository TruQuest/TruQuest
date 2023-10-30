using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface IThingValidationVerifierLotteryEventQueryable
{
    Task<string> GetJoinedEventUserDataFor(Guid thingId, string walletAddress);
    Task<(
        OrchestratorLotteryCommitmentQm?,
        LotteryClosedEventQm?,
        IEnumerable<VerifierLotteryParticipantEntryQm>
    )> GetOrchestratorCommitmentAndParticipants(Guid thingId);
}
