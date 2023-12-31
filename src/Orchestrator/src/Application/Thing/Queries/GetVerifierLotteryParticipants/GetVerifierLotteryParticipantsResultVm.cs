using Application.Common.Models.QM;

namespace Application.Thing.Queries.GetVerifierLotteryParticipants;

public class GetVerifierLotteryParticipantsResultVm
{
    public required Guid ThingId { get; init; }
    public required OrchestratorLotteryCommitmentQm? OrchestratorCommitment { get; init; }
    public required LotteryClosedEventQm? LotteryClosedEvent { get; init; }
    public required IEnumerable<VerifierLotteryParticipantEntryQm> Participants { get; init; }
}
