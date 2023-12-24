using Application.Admin.Queries.GetContractsStates.QM;

namespace Application.Admin.Queries.GetContractsStates.VM;

public class ThingValidationVerifierLotteryVm
{
    public required OrchestratorLotteryCommitmentQm OrchestratorCommitment { get; init; }
    public required IEnumerable<LotteryParticipantVm> Participants { get; init; }
}
