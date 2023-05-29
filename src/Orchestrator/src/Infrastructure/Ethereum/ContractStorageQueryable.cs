using Microsoft.Extensions.Configuration;

using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class ContractStorageQueryable : IContractStorageQueryable
{
    private readonly Contract _thingSubmissionVerifierLotteryContract;
    private readonly Contract _acceptancePollContract;
    private readonly Contract _thingAssessmentVerifierLotteryContract;
    private readonly Contract _assessmentPollContract;

    public ContractStorageQueryable(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"];
        _thingSubmissionVerifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("ThingSubmissionVerifierLottery")
            .DeployedAt(configuration[$"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"]!)
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

        _assessmentPollContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("AssessmentPoll")
            .DeployedAt(configuration[$"Ethereum:Contracts:{network}:AssessmentPoll:Address"]!)
            .OnNetwork(configuration[$"Ethereum:Networks:{network}:URL"]!)
            .Find();
    }

    public async Task<int> GetThingSubmissionVerifierLotteryDurationBlocks()
    {
        var value = await _thingSubmissionVerifierLotteryContract
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

    public async Task<int> GetThingSubmissionNumVerifiers()
    {
        var value = await _thingSubmissionVerifierLotteryContract
            .WalkStorage()
            .Field("s_numVerifiers")
            .GetValue<SolUint8>();

        return value.Value;
    }

    public async Task<string> GetThingSubmissionVerifierLotteryParticipantAt(byte[] thingId, int index)
    {
        var value = await _thingSubmissionVerifierLotteryContract
            .WalkStorage()
            .Field("s_participants")
            .AsMapping()
            .Key(new SolBytes16(thingId))
            .AsArrayOf<SolAddress>()
            .Index(index)
            .GetValue<SolAddress>();

        return value.Value;
    }

    public async Task<int> GetThingAcceptancePollVotingVolumeThreshold()
    {
        var value = await _acceptancePollContract
            .WalkStorage()
            .Field("s_votingVolumeThresholdPercent")
            .GetValue<SolUint8>();

        return value.Value;
    }

    public async Task<int> GetThingAcceptancePollMajorityThreshold()
    {
        var value = await _acceptancePollContract
            .WalkStorage()
            .Field("s_majorityThresholdPercent")
            .GetValue<SolUint8>();

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

    public async Task<string> GetThingAssessmentVerifierLotterySpotClaimantAt(
        byte[] thingId, byte[] proposalId, int index
    )
    {
        var value = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_claimants")
            .AsMapping()
            .Key(new SolBytes32(thingId.Concat(proposalId).ToArray()))
            .AsArrayOf<SolAddress>()
            .Index(index)
            .GetValue<SolAddress>();

        return value.Value;
    }

    public async Task<string> GetThingAssessmentVerifierLotteryParticipantAt(
        byte[] thingId, byte[] proposalId, int index
    )
    {
        var value = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_participants")
            .AsMapping()
            .Key(new SolBytes32(thingId.Concat(proposalId).ToArray()))
            .AsArrayOf<SolAddress>()
            .Index(index)
            .GetValue<SolAddress>();

        return value.Value;
    }

    public async Task<int> GetAssessmentPollDurationBlocks()
    {
        var value = await _assessmentPollContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        return value.Value;
    }
}