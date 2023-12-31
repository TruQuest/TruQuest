using Application.Common.Models.QM;

namespace Application.Settlement.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsResultVm
{
    public required Guid ProposalId { get; init; }
    public required OrchestratorLotteryCommitmentQm? OrchestratorCommitment { get; init; }
    public required LotteryClosedEventQm? LotteryClosedEvent { get; init; }
    public required IEnumerable<VerifierLotteryParticipantEntryQm> Participants { get; init; }
    public required IEnumerable<VerifierLotteryParticipantEntryQm> Claimants { get; init; }
}
