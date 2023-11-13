namespace Application.General.Queries.GetContractsStates.QM;

public class ThingValidationVerifierLotteryQm
{
    public required Guid ThingId { get; init; }
    public required OrchestratorLotteryCommitmentQm OrchestratorCommitment { get; init; }
    public required IEnumerable<LotteryParticipantQm> Participants { get; init; }
}
