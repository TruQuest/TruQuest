namespace Application.Admin.Queries.GetContractsStates.QM;

public class SettlementProposalAssessmentVerifierLotteryQm
{
    public required Guid ThingId { get; init; }
    public required Guid SettlementProposalId { get; init; }
    public required OrchestratorLotteryCommitmentQm OrchestratorCommitment { get; init; }
    public required IEnumerable<LotteryParticipantQm> Participants { get; init; }
    public required IEnumerable<LotteryParticipantQm> Claimants { get; init; }
}
