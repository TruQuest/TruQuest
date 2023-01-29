using System.Text.Json;
using System.Security.Cryptography;

using Xunit.Abstractions;
using FluentAssertions;
using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

using Application.Common.Interfaces;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Commands.CastAcceptancePollVote;
using Application.Settlement.Common.Models.IM;
using Application.Settlement.Commands.CreateNewSettlementProposalDraft;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using API.BackgroundServices;

namespace Tests.FunctionalTests;

[Collection(nameof(TruQuestTestCollection))]
public class E2ETests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly Sut _sut;

    private Contract _truQuestContract;
    private Contract _thingSubmissionVerifierLotteryContract;
    private Contract _acceptancePollContract;
    private Contract _thingAssessmentVerifierLotteryContract;
    private Contract _assessmentPollContract;

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

    public E2ETests(ITestOutputHelper output, Sut sut)
    {
        _output = output;
        _sut = sut;
        _dummyQuillContentJson = JsonSerializer.Serialize(_dummyQuillContent);
    }

    public async Task InitializeAsync()
    {
        await _sut.ResetState();

        await _sut.StartKafkaBus();
        await _sut.StartHostedService<BlockTracker>();
        await _sut.StartHostedService<ContractEventTracker>();

        var network = _sut.GetConfigurationValue<string>("Ethereum:Network");

        _truQuestContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("TruQuest")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:TruQuest:Address"))
            .OnNetwork(_sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL"))
            .Find();

        _thingSubmissionVerifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("ThingSubmissionVerifierLottery")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"))
            .OnNetwork(_sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL"))
            .Find();

        _acceptancePollContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("AcceptancePoll")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:AcceptancePoll:Address"))
            .OnNetwork(_sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL"))
            .Find();

        _thingAssessmentVerifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("ThingAssessmentVerifierLottery")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"))
            .OnNetwork(_sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL"))
            .Find();

        _assessmentPollContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("AssessmentPoll")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:AssessmentPoll:Address"))
            .OnNetwork(_sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL"))
            .Find();
    }

    public async Task DisposeAsync()
    {
        await _sut.StopHostedService<ContractEventTracker>();
        await _sut.StopHostedService<BlockTracker>();
    }

    [Fact]
    public async Task ShouldDoDo()
    {
        var network = _sut.GetConfigurationValue<string>("Ethereum:Network");

        var submitterAddress = _sut.AccountProvider.GetAccount("Submitter").Address.Substring(2).ToLower();

        Guid subjectId;
        using (var request = _sut.PrepareHttpRequestForFileUpload(
            fileNames: new[] { "full-image.jpg", "cropped-image-circle.png" },
            ("type", $"{(int)SubjectTypeIm.Person}"),
            ("name", "Alex Wurtz"),
            ("details", _dummyQuillContentJson),
            ("tags", "1|2|3")
        ))
        {
            _sut.RunAs(userId: submitterAddress, username: submitterAddress.Substring(2, 20));

            var subjectResult = await _sut.SendRequest(new AddNewSubjectCommand
            {
                Request = request.Request
            });

            subjectId = subjectResult.Data;
        }

        Guid thingId;
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

            thingId = thingDraftResult.Data;
        }

        await Task.Delay(TimeSpan.FromSeconds(30)); // giving time to archive attachments

        var thingSubmitResult = await _sut.SendRequest(new SubmitNewThingCommand
        {
            ThingId = thingId
        });

        var thingIdBytes = thingId.ToByteArray();

        await _sut.ContractCaller.FundThing(thingIdBytes, thingSubmitResult.Data!.Signature);

        var submitter = await _truQuestContract
            .WalkStorage()
            .Field("s_thingSubmitter")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .GetValue<SolAddress>();

        submitter.Value.ToLower().Should().Be(submitterAddress);

        await Task.Delay(TimeSpan.FromSeconds(25)); // @@TODO: Wait for LotteryInitiated event instead.

        for (int i = 1; i <= 10; ++i)
        {
            var verifierAccountName = $"Verifier{i}";
            var verifier = _sut.AccountProvider.GetAccount(verifierAccountName);

            var data = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _sut.ExecWithService<IContractCaller, byte[]>(async contractCaller =>
            {
                return await contractCaller.ComputeHashForThingSubmissionVerifierLottery(data);
            });

            await _sut.ContractCaller.PreJoinThingSubmissionVerifierLotteryAs(verifierAccountName, thingIdBytes, dataHash);
            await _sut.ContractCaller.JoinThingSubmissionVerifierLotteryAs(verifierAccountName, thingIdBytes, data);

            var committedDataHash = await _thingSubmissionVerifierLotteryContract
                .WalkStorage()
                .Field("s_thingIdToLotteryCommitments")
                .AsMapping()
                .Key(new SolBytes16(thingIdBytes))
                .AsMapping()
                .Key(new SolAddress(verifier.Address))
                .AsStruct("Commitment")
                .Field("dataHash")
                .GetValue<SolBytes32>();

            committedDataHash.Value.Should().Equal(dataHash);

            var revealed = await _thingSubmissionVerifierLotteryContract
                .WalkStorage()
                .Field("s_thingIdToLotteryCommitments")
                .AsMapping()
                .Key(new SolBytes16(thingIdBytes))
                .AsMapping()
                .Key(new SolAddress(verifier.Address))
                .AsStruct("Commitment")
                .Field("revealed")
                .GetValue<SolBool>();

            revealed.Value.Should().BeTrue();
        }

        await Task.Delay(TimeSpan.FromSeconds(15)); // giving time for (Pre-)Joined events to be handled.

        var lotteryDurationBlocks = await _thingSubmissionVerifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks.Value);

        await Task.Delay(TimeSpan.FromSeconds(15)); // giving time to close verifier lottery

        await _sut.BlockchainManipulator.Mine(1);
        // giving time to add verifiers, change the thing's state, and create an acceptance poll closing task
        await Task.Delay(TimeSpan.FromSeconds(30));

        // @@TODO: Check that winners are who they should be.

        int requiredVerifierCount = await _sut.ExecWithService<IContractStorageQueryable, int>(
            contractStorageQueryable => contractStorageQueryable.GetThingSubmissionNumVerifiers()
        );

        int verifierCount = await _acceptancePollContract
            .WalkStorage()
            .Field("s_thingVerifiers")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .AsArrayOf<SolAddress>()
            .Length();

        verifierCount.Should().Be(requiredVerifierCount);

        var thingSubmissionVerifierAccountNames = new List<string>();

        for (int i = 0; i < verifierCount; ++i)
        {
            var verifier = await _acceptancePollContract
                .WalkStorage()
                .Field("s_thingVerifiers")
                .AsMapping()
                .Key(new SolBytes16(thingIdBytes))
                .AsArrayOf<SolAddress>()
                .Index(i)
                .GetValue<SolAddress>();

            var verifierAccountName = _sut.AccountProvider.LookupNameByAddress(verifier.Value);

            thingSubmissionVerifierAccountNames.Add(verifierAccountName);

            _sut.RunAs(userId: verifier.Value.ToLower(), username: verifier.Value.ToLower().Substring(2, 20));

            var voteInput = new NewAcceptancePollVoteIm
            {
                ThingId = thingId,
                CastedAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:sszzz"),
                Decision = DecisionIm.Accept,
                Reason = "Some reason"
            };

            await _sut.SendRequest(new CastAcceptancePollVoteCommand
            {
                Input = voteInput,
                Signature = _sut.Signer.SignNewAcceptancePollVoteMessageAs(verifierAccountName, voteInput)
            });
        }

        var pollDurationBlocks = await _acceptancePollContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(pollDurationBlocks.Value + 10);

        await Task.Delay(TimeSpan.FromSeconds(15)); // giving time to finalize poll

        var pollStage = await _acceptancePollContract
            .WalkStorage()
            .Field("s_thingPollStage")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .GetValue<SolUint8>();

        pollStage.Value.Should().Be(4);

        var proposerAddress = _sut.AccountProvider.GetAccount("Proposer").Address.Substring(2).ToLower();

        Guid proposalId;
        using (var request = _sut.PrepareHttpRequestForFileUpload(
            fileNames: new[] { "full-image.jpg", "cropped-image-rect.png" },
            ("thingId", thingId.ToString()),
            ("title", "Go to the Moooooon..."),
            ("verdict", $"{(int)VerdictIm.NoEffortWhatsoever}"),
            ("details", _dummyQuillContentJson),
            ("evidence", "https://facebook.com")
        ))
        {
            _sut.RunAs(userId: proposerAddress, username: proposerAddress.Substring(2, 20));

            var proposalDraftResult = await _sut.SendRequest(new CreateNewSettlementProposalDraftCommand
            {
                Request = request.Request
            });

            proposalId = proposalDraftResult.Data;
        }

        await Task.Delay(TimeSpan.FromSeconds(30)); // giving time to archive attachments

        var proposalSubmitResult = await _sut.SendRequest(new SubmitNewSettlementProposalCommand
        {
            ProposalId = proposalId
        });

        var proposalIdBytes = proposalId.ToByteArray();

        await _sut.ContractCaller.FundThingSettlementProposal(
            thingIdBytes, proposalIdBytes, proposalSubmitResult.Data!.Signature
        );

        var proposer = await _truQuestContract
            .WalkStorage()
            .Field("s_thingIdToSettlementProposal")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .AsStruct("SettlementProposal")
            .Field("submitter")
            .GetValue<SolAddress>();

        proposer.Value.ToLower().Should().Be(proposerAddress);

        await Task.Delay(TimeSpan.FromSeconds(20)); // giving time to init thing assessment verifier lottery

        var thingProposalIdBytes = thingIdBytes.Concat(proposalIdBytes).ToArray();

        for (int i = 1; i <= 10; ++i)
        {
            var verifierAccountName = $"Verifier{i}";
            var verifier = _sut.AccountProvider.GetAccount(verifierAccountName);

            if (thingSubmissionVerifierAccountNames.Contains(verifierAccountName))
            {
                await _sut.ContractCaller.ClaimThingAssessmentVerifierLotterySpotAs(
                    verifierAccountName, thingProposalIdBytes
                );

                var committedDataHash = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingProposalIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes32(thingProposalIdBytes))
                    .AsMapping()
                    .Key(new SolAddress(verifier.Address))
                    .AsStruct("Commitment")
                    .Field("dataHash")
                    .GetValue<SolBytes32>();

                committedDataHash.Value.Should().Equal(new byte[32]);

                var revealed = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingProposalIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes32(thingProposalIdBytes))
                    .AsMapping()
                    .Key(new SolAddress(verifier.Address))
                    .AsStruct("Commitment")
                    .Field("revealed")
                    .GetValue<SolBool>();

                revealed.Value.Should().BeTrue();
            }
            else
            {
                var data = RandomNumberGenerator.GetBytes(32);
                var dataHash = await _sut.ExecWithService<IContractCaller, byte[]>(async contractCaller =>
                {
                    return await contractCaller.ComputeHashForThingAssessmentVerifierLottery(data);
                });

                await _sut.ContractCaller.PreJoinThingAssessmentVerifierLotteryAs(
                    verifierAccountName, thingProposalIdBytes, dataHash
                );
                await _sut.ContractCaller.JoinThingAssessmentVerifierLotteryAs(
                    verifierAccountName, thingProposalIdBytes, data
                );

                var committedDataHash = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingProposalIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes32(thingProposalIdBytes))
                    .AsMapping()
                    .Key(new SolAddress(verifier.Address))
                    .AsStruct("Commitment")
                    .Field("dataHash")
                    .GetValue<SolBytes32>();

                committedDataHash.Value.Should().Equal(dataHash);

                var revealed = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingProposalIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes32(thingProposalIdBytes))
                    .AsMapping()
                    .Key(new SolAddress(verifier.Address))
                    .AsStruct("Commitment")
                    .Field("revealed")
                    .GetValue<SolBool>();

                revealed.Value.Should().BeTrue();
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(20)); // giving time to handle Claimed/(Pre-)Joined events

        lotteryDurationBlocks = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks.Value);

        await Task.Delay(TimeSpan.FromSeconds(15)); // giving time to close verifier lottery

        await _sut.BlockchainManipulator.Mine(1);
        // giving time to add verifiers, change the proposal's state, and create an assessment poll closing task
        await Task.Delay(TimeSpan.FromSeconds(30));

        var orchestrator = _sut.AccountProvider.GetAccount("Orchestrator");

        var block = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_thingProposalIdToLotteryCommitments")
            .AsMapping()
            .Key(new SolBytes32(thingProposalIdBytes))
            .AsMapping()
            .Key(new SolAddress(orchestrator.Address))
            .AsStruct("Commitment")
            .Field("block")
            .GetValue<SolInt64>();

        block.Value.Should().Be(-1);

        // @@TODO: Check that winners are who they should be.

        requiredVerifierCount = await _sut.ExecWithService<IContractStorageQueryable, int>(
            contractStorageQueryable => contractStorageQueryable.GetThingAssessmentNumVerifiers()
        );

        verifierCount = await _assessmentPollContract
            .WalkStorage()
            .Field("s_proposalVerifiers")
            .AsMapping()
            .Key(new SolBytes32(thingProposalIdBytes))
            .AsArrayOf<SolAddress>()
            .Length();

        verifierCount.Should().Be(requiredVerifierCount);

        // var settlementProposalAssessmentVerifierAccountNames = new List<string>();

        // for (int i = 0; i < verifierCount; ++i)
        // {
        //     var verifier = await _assessmentPollContract
        //         .WalkStorage()
        //         .Field("s_proposalVerifiers")
        //         .AsMapping()
        //         .Key(new SolBytes32(thingProposalIdBytes))
        //         .AsArrayOf<SolAddress>()
        //         .Index(i)
        //         .GetValue<SolAddress>();

        //     var verifierAccountName = _sut.AccountProvider.LookupNameByAddress(verifier.Value);
        //     settlementProposalAssessmentVerifierAccountNames.Add(verifierAccountName);

        //     await _sut.ContractCaller.CastAssessmentPollVoteAs(verifierAccountName, thingProposalIdBytes, Vote.Accept);
        // }

        // await Task.Delay(TimeSpan.FromSeconds(15)); // giving time to handle CastedVote events

        // pollDurationBlocks = await _assessmentPollContract
        //     .WalkStorage()
        //     .Field("s_durationBlocks")
        //     .GetValue<SolUint16>();

        // await _sut.BlockchainManipulator.Mine(pollDurationBlocks.Value);

        // await Task.Delay(TimeSpan.FromSeconds(20)); // giving time to finalize poll

        // pollStage = await _assessmentPollContract
        //     .WalkStorage()
        //     .Field("s_proposalPollStage")
        //     .AsMapping()
        //     .Key(new SolBytes32(thingProposalIdBytes))
        //     .GetValue<SolUint8>();

        // pollStage.Value.Should().Be(4);
    }
}