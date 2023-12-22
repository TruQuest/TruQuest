using Application.General.Queries.GetContractsStates.QM;

namespace Application.General.Queries.GetContractsStates.VM;

public class GetContractsStatesRvm
{
    public required TruQuestContractInfoQm TruQuestInfo { get; init; }
    public required IEnumerable<string> WhitelistedWalletAddresses { get; init; }
    public required IEnumerable<UserVm> Users { get; init; }
    public required IEnumerable<SubjectVm> Subjects { get; init; }
}
