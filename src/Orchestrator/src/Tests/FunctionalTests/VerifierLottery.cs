using System.Security.Cryptography;

using Application.Common.Interfaces;
using Application.Common.Models.IM;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;
using API.BackgroundServices;

namespace Tests.FunctionalTests;

[Collection(nameof(TruQuestTestCollection))]
public class VerifierLottery : IAsyncLifetime
{
    private readonly Sut _sut;

    public VerifierLottery(Sut sut)
    {
        _sut = sut;
    }

    public async Task InitializeAsync()
    {
        await _sut.ResetState();

        await _sut.StartKafkaBus();
        await _sut.StartHostedService<BlockTracker>();
        await _sut.StartHostedService<ContractEventTracker>();

        var network = _sut.GetConfigurationValue<string>("Ethereum:Network");
        var players = new[]
        {
            "Player",
            "LotteryPlayer1",
            "LotteryPlayer2",
            "LotteryPlayer3",
            "LotteryPlayer4"
        };

        foreach (var player in players)
        {
            await _sut.ContractCaller.TransferTruthserumTo(
                _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:{player}:Address"),
                500
            );
            await _sut.ContractCaller.DepositFunds(
                _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:{player}:PrivateKey"),
                500
            );
        }
    }

    public async Task DisposeAsync()
    {
        await _sut.StopHostedService<ContractEventTracker>();
        await _sut.StopHostedService<BlockTracker>();
    }

    [Fact]
    public async Task ShouldDoDo()
    {
        var subjectInput = new NewSubjectIm
        {
            Type = SubjectTypeIm.Person,
            Name = "Name",
            Details = "Details",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b6/Image_created_with_a_mobile_phone.png/640px-Image_created_with_a_mobile_phone.png",
            Tags = new List<TagIm>() { new() { Id = 1 } }
        };

        var sig = _sut.Signer.SignNewSubjectMessage(subjectInput);

        _sut.RunAs(userId: "bF2Ff171C3C4A63FBBD369ddb021c75934005e81", username: "player");

        var subjectResult = await _sut.SendRequest(new AddNewSubjectCommand
        {
            Input = subjectInput,
            Signature = sig
        });

        var subjectId = subjectResult.Data;

        var thingInput = new NewThingIm
        {
            SubjectId = subjectId,
            Title = "Moon base",
            Details = "2024",
            ImageUrl = "https://images.newscientist.com/wp-content/uploads/2022/09/09152048/SEI_124263525.jpg",
            Evidence = new List<EvidenceIm>()
            {
                new() { Url = "https://stackoverflow.com/" },
                new() { Url = "https://fanfics.me/" }
            },
            Tags = new List<TagIm>() { new() { Id = 1 } }
        };

        sig = _sut.Signer.SignNewThingMessage(thingInput);

        var thingResult = await _sut.SendRequest(new SubmitNewThingCommand
        {
            Input = thingInput,
            Signature = sig
        });

        await _sut.ContractCaller.FundThing(thingResult.Data!.Thing, thingResult.Data.Signature);

        var thingId = thingResult.Data.Thing.Id;

        await Task.Delay(TimeSpan.FromSeconds(10));

        var network = _sut.GetConfigurationValue<string>("Ethereum:Network");
        for (int i = 1; i <= 4; ++i)
        {
            var privateKey = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:LotteryPlayer{i}:PrivateKey");
            var data = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _sut.ExecWithService<IContractCaller, byte[]>(async contractCaller =>
            {
                return await contractCaller.ComputeHash(data);
            });

            await _sut.ContractCaller.PreJoinLotteryAs(privateKey, thingId, dataHash);
            await _sut.ContractCaller.JoinLotteryAs(privateKey, thingId, data);
        }

        await Task.Delay(TimeSpan.FromSeconds(10));

        int lotteryDurationBlocks = await _sut.ContractCaller.GetLotteryDurationBlocks();
        long lotteryInitBlockNumber = await _sut.ContractCaller.GetLotteryInitBlockNumber(thingId);

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks);

        await Task.Delay(TimeSpan.FromSeconds(10));

        await _sut.BlockchainManipulator.Mine(1);

        await Task.Delay(TimeSpan.FromSeconds(20));
    }
}