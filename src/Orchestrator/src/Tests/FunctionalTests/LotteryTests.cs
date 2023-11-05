using System.Security.Cryptography;
using System.Diagnostics;

using Nito.AsyncEx;
using FluentAssertions;
using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

using Application.Common.Interfaces;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Commands.SubmitNewThing;
using Application.Ethereum.Events.ThingValidationVerifierLottery.LotteryClosedInFailure;
using Application.Thing.Queries.GetThing;
using Application.Common.Models.QM;

namespace Tests.FunctionalTests;

public class LotteryTests : BaseTests
{
    private Contract _truQuestContract;
    private Contract _thingValidationVerifierLotteryContract;
    private Contract _thingValidationPollContract;

    private Guid _thingId;
    private string _submitterId;
    private long _submitterInitialBalance;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var network = _sut.GetConfigurationValue<string>("Ethereum:Network");
        var rpcUrl = _sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL");

        _truQuestContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("TruQuest")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:TruQuest:Address"))
            .OnNetwork(rpcUrl)
            .Find();

        _thingValidationVerifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("ThingValidationVerifierLottery")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:ThingValidationVerifierLottery:Address"))
            .OnNetwork(rpcUrl)
            .Find();

        _thingValidationPollContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("ThingValidationPoll")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:ThingValidationPoll:Address"))
            .OnNetwork(rpcUrl)
            .Find();

        await _initializeThingLottery();
    }

    private async Task _initializeThingLottery()
    {
        Guid subjectId;
        using (var request = _sut.PrepareHttpRequestForFileUpload(
            fileNames: new[] { "full-image.jpg", "cropped-image-circle.png" },
            ("type", $"{(int)SubjectTypeIm.Person}"),
            ("name", "Alex Wurtz"),
            ("details", _dummyQuillContentJson),
            ("tags", "1|2|3")
        ))
        {
            await _runAs(accountName: "Proposer");

            var subjectResult = await _sendRequest(new AddNewSubjectCommand(request.Request));

            subjectId = subjectResult.Data;
        }

        var draftCreatedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingDraftCreated += delegate
        {
            draftCreatedTcs.SetResult();
        };

        using (var request = _sut.PrepareHttpRequestForFileUpload(
            fileNames: new[] { "full-image.jpg", "cropped-image-rect.png" },
            ("subjectId", subjectId.ToString()),
            ("title", "Go to the Moooooon..."),
            ("details", _dummyQuillContentJson),
            ("evidence", "https://ycombinator.com|https://habr.com|https://sports.ru"),
            ("tags", "1|2|3")
        ))
        {
            var thingDraftResult = await _sendRequest(new CreateNewThingDraftCommand(request.Request));
            _thingId = thingDraftResult.Data;

            _eventBroadcaster.SetThingOfInterest(_thingId);
        }

        await draftCreatedTcs.Task;

        var thingSubmitResult = await _sendRequest(new SubmitNewThingCommand
        {
            ThingId = _thingId
        });

        var thingStake = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingStake")
                .GetValue<SolUint256>()
        ).Value;

        _submitterInitialBalance = await _sut.ContractCaller.GetAvailableFunds("Proposer");

        var lotteryInitializedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingValidationVerifierLotteryInitialized += delegate
        {
            lotteryInitializedTcs.SetResult();
        };

        var thingIdBytes = _thingId.ToByteArray();

        await _sut.ContractCaller.FundThingAs("Proposer", thingIdBytes, thingSubmitResult.Data!.Signature);

        var submitterBalance = await _sut.ContractCaller.GetAvailableFunds("Proposer");
        submitterBalance.Should().Be(_submitterInitialBalance - thingStake);

        var submitterAddress = await _sut.ContractCaller.GetWalletAddressFor("Proposer");

        var submitter = await _truQuestContract
            .WalkStorage()
            .Field("s_thingSubmitter")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .GetValue<SolAddress>();

        submitter.Value.ToLower().Should().Be(submitterAddress.Substring(2).ToLower());

        await lotteryInitializedTcs.Task;
    }

    [Fact]
    public async Task LotteryShouldFailWhenNotEnoughParticipantsJoined()
    {
        var thingIdBytes = _thingId.ToByteArray();

        int requiredVerifierCount = await _sut.ExecWithService<IContractCaller, int>(
            contractCaller => contractCaller.GetThingValidationVerifierLotteryNumVerifiers()
        );

        var verifierCount = requiredVerifierCount - 1;

        Debug.WriteLine($"Required verifiers: {requiredVerifierCount}. Actual verifiers: {verifierCount}");

        var cde = new AsyncCountdownEvent(verifierCount);
        _eventBroadcaster.JoinedThingValidationVerifierLottery += delegate
        {
            cde.Signal(1);
        };

        var lotteryClosedTcs = new TaskCompletionSource<LotteryClosedInFailureEvent>();
        _eventBroadcaster.ThingValidationVerifierLotteryClosedInFailure += (_, @event) =>
        {
            lotteryClosedTcs.SetResult(@event.Event);
        };

        var verifiersLotteryData = new List<(
            string AccountName,
            string WalletAddress,
            long InitialBalance,
            byte[] UserData,
            long? Nonce
        )>();

        var verifierStake = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_verifierStake")
                .GetValue<SolUint256>()
        ).Value;

        for (int i = 1; i <= verifierCount; ++i)
        {
            var verifierAccountName = $"Verifier{i + 3}";

            var userData = RandomNumberGenerator.GetBytes(32);

            var verifierInitialBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);

            await _sut.ContractCaller.JoinThingValidationVerifierLotteryAs(verifierAccountName, thingIdBytes, userData);

            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);
            verifierBalance.Should().Be(verifierInitialBalance - verifierStake);

            var walletAddress = await _sut.ContractCaller.GetWalletAddressFor(verifierAccountName);

            verifiersLotteryData.Add((verifierAccountName, walletAddress, verifierInitialBalance, userData, null));
        }

        await cde.WaitAsync();

        var lotteryDurationBlocks = await _sut.ExecWithService<IContractCaller, int>(contractCaller =>
            contractCaller.GetThingValidationVerifierLotteryDurationBlocks()
        );

        var thingArchivedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingArchived += delegate
        {
            thingArchivedTcs.SetResult();
        };

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks);

        var lotteryClosedInFailureEvent = await lotteryClosedTcs.Task;

        lotteryClosedInFailureEvent.RequiredNumVerifiers.Should().Be(requiredVerifierCount);
        lotteryClosedInFailureEvent.JoinedNumVerifiers.Should().Be(verifierCount);

        // All verifiers should be unstaked.
        foreach (var verifierData in verifiersLotteryData)
        {
            var balance = await _sut.ContractCaller.GetAvailableFunds(verifierData.AccountName);
            balance.Should().Be(verifierData.InitialBalance);
        }

        // Submitter should also be unstaked without any penalty.
        var submitterBalance = await _sut.ContractCaller.GetAvailableFunds("Proposer");
        submitterBalance.Should().Be(_submitterInitialBalance);

        await thingArchivedTcs.Task;

        // Thing should be archived.
        var thing = await _sut.ExecWithService<IThingQueryable, ThingQm?>(thingQueryable =>
            thingQueryable.GetById(_thingId, _submitterId)
        );

        thing!.State.Should().Be(ThingStateQm.VerifierLotteryFailed);
        thing.RelatedThings!.Should().ContainKey("next");

        Debug.WriteLine($"*************** Lottery failed: not enough participants ***************");
    }
}
