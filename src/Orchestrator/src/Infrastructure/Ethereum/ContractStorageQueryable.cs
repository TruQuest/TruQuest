using Microsoft.Extensions.Configuration;

using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

using Application.Common.Interfaces;

namespace Infrastructure.Ethereum;

internal class ContractStorageQueryable : IContractStorageQueryable
{
    private readonly Contract _verifierLotteryContract;

    public ContractStorageQueryable(IConfiguration configuration)
    {
        var network = configuration["Ethereum:Network"];
        _verifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("VerifierLottery")
            .DeployedAt(configuration[$"Ethereum:Contracts:{network}:VerifierLottery:Address"]!)
            .OnNetwork(configuration[$"Ethereum:Networks:{network}:URL"]!)
            .Find();
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
}