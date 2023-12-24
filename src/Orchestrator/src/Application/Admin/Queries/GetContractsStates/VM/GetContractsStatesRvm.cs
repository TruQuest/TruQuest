using Application.Admin.Queries.GetContractsStates.QM;

namespace Application.Admin.Queries.GetContractsStates.VM;

public class GetContractsStatesRvm
{
    public required TruQuestContractInfoQm TruQuestInfo { get; init; }
    public required IEnumerable<string> WhitelistedWalletAddresses { get; init; }
    public required IEnumerable<UserVm> Users { get; init; }
    public required IEnumerable<SubjectVm> Subjects { get; init; }
}
