using Microsoft.Extensions.Configuration;

using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class ContractStorageQueryable : IContractStorageQueryable
{
    private readonly Contract _verifierLotteryContract;
    private readonly Contract _acceptancePollContract;
    private readonly Contract _thingAssessmentVerifierLotteryContract;

    public ContractStorageQueryable(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"];
        _verifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("VerifierLottery")
            .DeployedAt(configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!)
            .OnNetwork(configuration[$"Ethereum:Networks:{network}:URL"]!)
            .Find();

        _acceptancePollContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("AcceptancePoll")
            .DeployedAt(configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!)
            .OnNetwork(configuration[$"Ethereum:Networks:{network}:URL"]!)
            .Find();

        _thingAssessmentVerifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("ThingAssessmentVerifierLottery")
            .DeployedAt(configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!)
            .OnNetwork(configuration[$"Ethereum:Networks:{network}:URL"]!)
            .Find();
    }

    public async Task<int> GetThingSubmissionVerifierLotteryDurationBlocks()
    {
        var value = await _verifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        return value.Value;
    }

    public async Task<int> GetAcceptancePollDurationBlocks()
    {
        var value = await _acceptancePollContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        return value.Value;
    }

    public async Task<int> GetNumVerifiers()
    {
        var value = await _verifierLotteryContract
            .WalkStorage()
            .Field("s_numVerifiers")
            .GetValue<SolUint8>();

        return value.Value;
    }

    public async Task<string> GetVerifierLotteryParticipantAt(string thingId, int index)
    {
        var value = await _verifierLotteryContract
            .WalkStorage()
            .Field("s_participants")
            .AsMapping()
            .Key(new SolString(thingId))
            .AsArrayOf<SolAddress>()
            .Index(index)
            .GetValue<SolAddress>();

        return value.Value;
    }

    public async Task<int> GetThingAssessmentVerifierLotteryDurationBlocks()
    {
        var value = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        return value.Value;
    }

    public async Task<int> GetThingAssessmentNumVerifiers()
    {
        var value = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_numVerifiers")
            .GetValue<SolUint8>();

        return value.Value;
    }

    public async Task<string> GetThingAssessmentVerifierLotterySpotClaimantAt(string thingId, int index)
    {
        var value = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_claimants")
            .AsMapping()
            .Key(new SolString(thingId))
            .AsArrayOf<SolAddress>()
            .Index(index)
            .GetValue<SolAddress>();

        return value.Value;
    }

    public async Task<string> GetThingAssessmentVerifierLotteryParticipantAt(string thingId, int index)
    {
        var value = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_participants")
            .AsMapping()
            .Key(new SolString(thingId))
            .AsArrayOf<SolAddress>()
            .Index(index)
            .GetValue<SolAddress>();

        return value.Value;
    }
}