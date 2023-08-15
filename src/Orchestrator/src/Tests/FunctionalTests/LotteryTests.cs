using System.Text.Json;
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
using Application.Ethereum.Events.ThingSubmissionVerifierLottery.LotteryClosedInFailure;
using Application.Thing.Queries.GetThing;
using Application.Common.Models.QM;
using API.BackgroundServices;

using Tests.FunctionalTests.Helpers;

namespace Tests.FunctionalTests;

[Collection(nameof(TruQuestTestCollection))]
public class LotteryTests : IAsyncLifetime
{
    private readonly Sut _sut;
    private EventBroadcaster _eventBroadcaster;

    private Contract _truQuestContract;
    private Contract _thingSubmissionVerifierLotteryContract;
    private Contract _acceptancePollContract;

    private readonly string _dummyQuillContentJson;
    private readonly List<Dictionary<string, object>> _dummyQuillContent = new()
    {
        new()
        {
            ["insert"] = "Hello World!"
        },
        new()
        {
            ["attributes"] = new Dictionary<string, object>()
            {
                ["header"] = 1
            },
            ["insert"] = "\n"
        },
        new()
        {
            ["insert"] = "Welcome to TruQuest!"
        },
        new()
        {
            ["attributes"] = new Dictionary<string, object>()
            {
                ["header"] = 2
            },
            ["insert"] = "\n"
        }
    };

    private Guid _thingId;
    private string _submitterId;
    private long _submitterInitialBalance;

    public LotteryTests(Sut sut)
    {
        _sut = sut;
        _dummyQuillContentJson = JsonSerializer.Serialize(_dummyQuillContent);
    }

    public async Task InitializeAsync()
    {
        await _sut.ResetState();

        _eventBroadcaster = new EventBroadcaster(_sut.ApplicationEventSink.Stream, _sut.ApplicationRequestSink.Stream);
        _eventBroadcaster.Start();

        await _sut.StartKafkaBus();
        await _sut.StartHostedService<BlockTracker>();
        await _sut.StartHostedService<ContractEventTracker>();

        var network = _sut.GetConfigurationValue<string>("Ethereum:Network");
        var rpcUrl = _sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL");

        _truQuestContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("TruQuest")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:TruQuest:Address"))
            .OnNetwork(rpcUrl)
            .Find();

        _thingSubmissionVerifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("ThingSubmissionVerifierLottery")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"))
            .OnNetwork(rpcUrl)
            .Find();

        _acceptancePollContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("AcceptancePoll")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:AcceptancePoll:Address"))
            .OnNetwork(rpcUrl)
            .Find();

        await _initializeThingLottery();
    }

    public async Task DisposeAsync()
    {
        await _sut.StopHostedService<ContractEventTracker>();
        await _sut.StopHostedService<BlockTracker>();
        _eventBroadcaster.Stop();
    }

    private async Task _initializeThingLottery()
    {
        var submitterAddress = await _sut.ContractCaller.GetWalletAddressFor("Submitter");
        _submitterId = submitterAddress.Substring(2).ToLower();

        Guid subjectId;
        using (var request = _sut.PrepareHttpRequestForFileUpload(
            fileNames: new[] { "full-image.jpg", "cropped-image-circle.png" },
            ("type", $"{(int)SubjectTypeIm.Person}"),
            ("name", "Alex Wurtz"),
            ("details", _dummyQuillContentJson),
            ("tags", "1|2|3")
        ))
        {
            _sut.RunAs(userId: _submitterId, username: submitterAddress);

            var subjectResult = await _sut.SendRequest(new AddNewSubjectCommand
            {
                Request = request.Request
            });

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
            ("evidence", "https://google.com"),
            ("tags", "1|2|3")
        ))
        {
            var thingDraftResult = await _sut.SendRequest(new CreateNewThingDraftCommand
            {
                Request = request.Request
            });

            _thingId = thingDraftResult.Data;
        }

        await draftCreatedTcs.Task;

        var thingSubmitResult = await _sut.SendRequest(new SubmitNewThingCommand
        {
            ThingId = _thingId
        });

        var thingSubmissionStake = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingSubmissionStake")
                .GetValue<SolUint256>()
        ).Value;

        _submitterInitialBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");

        var lotteryInitializedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingSubmissionVerifierLotteryInitialized += delegate
        {
            lotteryInitializedTcs.SetResult();
        };

        var thingIdBytes = _thingId.ToByteArray();

        await _sut.ContractCaller.FundThingAs("Submitter", thingIdBytes, thingSubmitResult.Data!.Signature);

        var submitterBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");
        submitterBalance.Should().Be(_submitterInitialBalance - thingSubmissionStake);

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
            contractCaller => contractCaller.GetThingSubmissionLotteryNumVerifiers()
        );

        var verifierCount = requiredVerifierCount - 1;

        Debug.WriteLine($"Required verifiers: {requiredVerifierCount}. Actual verifiers: {verifierCount}");

        var cde = new AsyncCountdownEvent(verifierCount);
        _eventBroadcaster.JoinedThingSubmissionVerifierLottery += delegate
        {
            cde.Signal(1);
        };

        var thingLotteryClosedTcs = new TaskCompletionSource<LotteryClosedInFailureEvent>();
        _eventBroadcaster.ThingSubmissionVerifierLotteryClosedInFailure += (_, @event) =>
        {
            thingLotteryClosedTcs.SetResult(@event.Event);
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
            var verifierAccountName = $"Verifier{i}";

            var userData = RandomNumberGenerator.GetBytes(32);

            var verifierInitialBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);

            await _sut.ContractCaller.JoinThingSubmissionVerifierLotteryAs(verifierAccountName, thingIdBytes, userData);

            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);
            verifierBalance.Should().Be(verifierInitialBalance - verifierStake);

            var walletAddress = await _sut.ContractCaller.GetWalletAddressFor(verifierAccountName);

            verifiersLotteryData.Add((verifierAccountName, walletAddress, verifierInitialBalance, userData, null));
        }

        await cde.WaitAsync();

        var lotteryDurationBlocks = await _sut.ExecWithService<IContractCaller, int>(contractCaller =>
            contractCaller.GetThingSubmissionVerifierLotteryDurationBlocks()
        );

        var thingArchivedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingArchived += delegate
        {
            thingArchivedTcs.SetResult();
        };

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks);

        var thingLotteryClosedInFailureEvent = await thingLotteryClosedTcs.Task;

        thingLotteryClosedInFailureEvent.RequiredNumVerifiers.Should().Be(requiredVerifierCount);
        thingLotteryClosedInFailureEvent.JoinedNumVerifiers.Should().Be(verifierCount);

        // All verifiers should be unstaked.
        foreach (var verifierData in verifiersLotteryData)
        {
            var balance = await _sut.ContractCaller.GetAvailableFunds(verifierData.AccountName);
            balance.Should().Be(verifierData.InitialBalance);
        }

        // Submitter should also be unstaked without any penalty.
        var submitterBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");
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
