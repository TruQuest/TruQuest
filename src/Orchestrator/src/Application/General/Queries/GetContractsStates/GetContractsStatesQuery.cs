using GoThataway;

using Domain.Results;

using Application.Common.Interfaces;

namespace Application.General.Queries.GetContractsStates;

public class GetContractsStatesQuery : IRequest<HandleResult<GetContractsStatesRvm>> { }

public class GetContractsStatesQueryHandler : IRequestHandler<GetContractsStatesQuery, HandleResult<GetContractsStatesRvm>>
{
    private readonly IContractCaller _contractCaller;

    public GetContractsStatesQueryHandler(IContractCaller contractCaller)
    {
        _contractCaller = contractCaller;
    }

    public async Task<HandleResult<GetContractsStatesRvm>> Handle(GetContractsStatesQuery query, CancellationToken ct)
    {
        return new()
        {
            Data = new()
            {
                WhitelistedUsers = await _contractCaller.GetRestrictedAccessWhitelist(),
                UserBalances = await _contractCaller.ExportUsersAndBalances(),
                ThingSubmitters = await _contractCaller.ExportThingSubmitter(),
                SettlementProposalSubmitters = await _contractCaller.ExportThingIdToSettlementProposal(),
                ThingValidationVerifierLotteries = await _contractCaller.ExportThingValidationVerifierLotteryData(),
                ThingValidationPolls = await _contractCaller.ExportThingValidationPollData(),
                SettlementProposalAssessmentVerifierLotteries = await _contractCaller.ExportSettlementProposalAssessmentVerifierLotteryData(),
                SettlementProposalAssessmentPolls = await _contractCaller.ExportSettlementProposalAssessmentPollData()
            }
        };
    }
}
