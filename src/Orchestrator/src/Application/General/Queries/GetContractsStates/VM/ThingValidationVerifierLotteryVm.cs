using Application.General.Queries.GetContractsStates.QM;

namespace Application.General.Queries.GetContractsStates.VM;

public class ThingValidationVerifierLotteryVm
{
    public required OrchestratorLotteryCommitmentQm OrchestratorCommitment { get; init; }
    public required IEnumerable<LotteryParticipantVm> Participants { get; init; }
}
