using Application.General.Queries.GetContractsStates.QM;

namespace Application.General.Queries.GetContractsStates.VM;

public class SettlementProposalAssessmentVerifierLotteryVm
{
    public required OrchestratorLotteryCommitmentQm OrchestratorCommitment { get; init; }
    public required IEnumerable<LotteryParticipantVm> Claimants { get; init; }
    public required IEnumerable<LotteryParticipantVm> Participants { get; init; }
}
