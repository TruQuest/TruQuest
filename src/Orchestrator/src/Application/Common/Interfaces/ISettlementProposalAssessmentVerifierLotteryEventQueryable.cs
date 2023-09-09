using Application.Common.Models.QM;

namespace Application.Common.Interfaces;

public interface ISettlementProposalAssessmentVerifierLotteryEventQueryable
{
    Task<(
        OrchestratorLotteryCommitmentQm?,
        LotteryClosedEventQm?,
        IEnumerable<VerifierLotteryParticipantEntryQm> Participants,
        IEnumerable<VerifierLotteryParticipantEntryQm> Claimants
    )> GetOrchestratorCommitmentAndParticipants(Guid thingId, Guid proposalId);
}
