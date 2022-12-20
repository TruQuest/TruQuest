using System.Security.Cryptography;

using Xunit.Abstractions;
using FluentAssertions;
using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

using Application.Common.Interfaces;
using Application.Common.Models.IM;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.SubmitNewThing;
using Application.Vote.Commands.CastVote;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using API.BackgroundServices;

namespace Tests.FunctionalTests;

[Collection(nameof(TruQuestTestCollection))]
public class E2ETests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly Sut _sut;

    private Contract _truQuestContract;
    private Contract _verifierLotteryContract;
    private Contract _acceptancePollContract;
    private Contract _thingAssessmentVerifierLotteryContract;

    private readonly Dictionary<string, string> _addressToPrivateKey = new();

    public E2ETests(ITestOutputHelper output, Sut sut)
    {
        _output = output;
        _sut = sut;
    }

    public async Task InitializeAsync()
    {
        await _sut.ResetState();

        await _sut.StartKafkaBus();
        await _sut.StartHostedService<BlockTracker>();
        await _sut.StartHostedService<ContractEventTracker>();

        var network = _sut.GetConfigurationValue<string>("Ethereum:Network");
        var users = new[]
        {
            "Submitter",
            "Proposer",
            "Verifier1",
            "Verifier2",
            "Verifier3",
            "Verifier4",
            "Verifier5",
            "Verifier6",
            "Verifier7",
            "Verifier8",
            "Verifier9",
            "Verifier10",
        };

        foreach (var user in users)
        {
            var address = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:{user}:Address");
            var privateKey = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:{user}:PrivateKey");
            await _sut.ContractCaller.TransferTruthserumTo(address, 500);
            await _sut.ContractCaller.DepositFundsAs(privateKey, 500);

            _addressToPrivateKey[address.Substring(2).ToLower()] = privateKey;
        }

        _truQuestContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("TruQuest")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:TruQuest:Address"))
            .OnNetwork(_sut.GetConfigurationValue<string>($"Ethereum:Networks:{network}:URL"))
            .Find();

        _verifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("VerifierLottery")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:VerifierLottery:Address"))
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

        var subjectInput = new NewSubjectIm
        {
            Type = SubjectTypeIm.Person,
            Name = "Name",
            Details = "Details",
            ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/b6/Image_created_with_a_mobile_phone.png/640px-Image_created_with_a_mobile_phone.png",
            Tags = new List<TagIm>() { new() { Id = 1 } }
        };

        var submitterAddress = _sut.GetConfigurationValue<string>(
            $"Ethereum:Accounts:{network}:Submitter:Address"
        ).Substring(2).ToLower();

        _sut.RunAs(userId: submitterAddress, username: submitterAddress.Substring(0, 20));

        var subjectResult = await _sut.SendRequest(new AddNewSubjectCommand
        {
            Input = subjectInput,
            Signature = _sut.Signer.SignNewSubjectMessage(subjectInput)
        });

        Guid subjectId = subjectResult.Data;

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

        var thingResult = await _sut.SendRequest(new SubmitNewThingCommand
        {
            Input = thingInput,
            Signature = _sut.Signer.SignNewThingMessage(thingInput)
        });

        await _sut.ContractCaller.FundThing(thingResult.Data!.Thing, thingResult.Data.Signature);

        byte[] thingId = thingResult.Data.Thing.Id.ToByteArray();

        var submitter = await _truQuestContract
            .WalkStorage()
            .Field("s_thingSubmitter")
            .AsMapping()
            .Key(new SolBytes16(thingId))
            .GetValue<SolAddress>();

        submitter.Value.ToLower().Should().Be(submitterAddress);

        await Task.Delay(TimeSpan.FromSeconds(15)); // @@TODO: Wait for LotteryInitiated event instead.

        for (int i = 1; i <= 10; ++i)
        {
            var privateKey = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:Verifier{i}:PrivateKey");
            var data = RandomNumberGenerator.GetBytes(32);
            var dataHash = await _sut.ExecWithService<IContractCaller, byte[]>(async contractCaller =>
            {
                return await contractCaller.ComputeHashForThingSubmissionVerifierLottery(data);
            });

            await _sut.ContractCaller.PreJoinLotteryAs(privateKey, thingId, dataHash);
            await _sut.ContractCaller.JoinLotteryAs(privateKey, thingId, data);

            var address = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:Verifier{i}:Address");
            var committedDataHash = await _verifierLotteryContract
                .WalkStorage()
                .Field("s_thingIdToLotteryCommitments")
                .AsMapping()
                .Key(new SolBytes16(thingId))
                .AsMapping()
                .Key(new SolAddress(address))
                .AsStruct("Commitment")
                .Field("dataHash")
                .GetValue<SolBytes32>();

            committedDataHash.Value.Should().Equal(dataHash);

            var revealed = await _verifierLotteryContract
                .WalkStorage()
                .Field("s_thingIdToLotteryCommitments")
                .AsMapping()
                .Key(new SolBytes16(thingId))
                .AsMapping()
                .Key(new SolAddress(address))
                .AsStruct("Commitment")
                .Field("revealed")
                .GetValue<SolBool>();

            revealed.Value.Should().BeTrue();
        }

        await Task.Delay(TimeSpan.FromSeconds(15)); // giving time for (Pre-)Joined events to be handled.

        var lotteryDurationBlocks = await _verifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks.Value);

        await Task.Delay(TimeSpan.FromSeconds(15)); // giving time to close verifier lottery

        await _sut.BlockchainManipulator.Mine(1);
        // giving time to add verifiers, change the thing's state, and create an acceptance poll closing task
        await Task.Delay(TimeSpan.FromSeconds(30));

        // check that winners are who they should be

        int requiredVerifierCount = await _sut.ExecWithService<IContractStorageQueryable, int>(
            contractStorageQueryable => contractStorageQueryable.GetNumVerifiers()
        );

        int verifierCount = await _acceptancePollContract
            .WalkStorage()
            .Field("s_thingVerifiers")
            .AsMapping()
            .Key(new SolBytes16(thingId))
            .AsArrayOf<SolAddress>()
            .Length();

        verifierCount.Should().Be(requiredVerifierCount);

        var thingSubmissionVerifiers = new List<string>();

        for (int i = 0; i < verifierCount; ++i)
        {
            var verifier = await _acceptancePollContract
                .WalkStorage()
                .Field("s_thingVerifiers")
                .AsMapping()
                .Key(new SolBytes16(thingId))
                .AsArrayOf<SolAddress>()
                .Index(i)
                .GetValue<SolAddress>();

            thingSubmissionVerifiers.Add(verifier.Value.ToLower());

            _sut.RunAs(userId: verifier.Value.ToLower(), username: verifier.Value.ToLower().Substring(0, 20));

            var voteInput = new NewVoteIm
            {
                ThingId = new Guid(thingId),
                PollType = PollTypeIm.Acceptance,
                CastedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                Decision = DecisionIm.Accept,
                Reason = "Some reason"
            };

            await _sut.SendRequest(new CastVoteCommand
            {
                Input = voteInput,
                Signature = _sut.Signer.SignNewVoteMessage(voteInput)
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
            .Key(new SolBytes16(thingId))
            .GetValue<SolUint8>();

        pollStage.Value.Should().Be(4);

        var proposerAddress = _sut.GetConfigurationValue<string>(
            $"Ethereum:Accounts:{network}:Proposer:Address"
        ).Substring(2).ToLower();

        _sut.RunAs(userId: proposerAddress, username: proposerAddress.Substring(0, 20));

        var proposalInput = new NewSettlementProposalIm
        {
            ThingId = new Guid(thingId),
            Title = "Proposal title",
            Verdict = VerdictIm.AintGoodEnough,
            Details = "Proposal details",
            Evidence = new List<SupportingEvidenceIm>
            {
                new() { Url = "https://www.nuget.org/" }
            }
        };

        var proposalResult = await _sut.SendRequest(new SubmitNewSettlementProposalCommand
        {
            Input = proposalInput,
            Signature = _sut.Signer.SignNewSettlementProposalMessage(proposalInput)
        });

        await _sut.ContractCaller.FundThingSettlementProposal(
            proposalResult.Data!.SettlementProposal, proposalResult.Data.Signature
        );

        var proposer = await _truQuestContract
            .WalkStorage()
            .Field("s_thingIdToSettlementProposal")
            .AsMapping()
            .Key(new SolBytes16(thingId))
            .AsStruct("SettlementProposal")
            .Field("submitter")
            .GetValue<SolAddress>();

        proposer.Value.ToLower().Should().Be(proposerAddress);

        await Task.Delay(TimeSpan.FromSeconds(20)); // giving time to init thing assessment verifier lottery

        for (int i = 1; i <= 10; ++i)
        {
            var address = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:Verifier{i}:Address");
            var privateKey = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:Verifier{i}:PrivateKey");

            if (thingSubmissionVerifiers.Contains(address.Substring(2).ToLower()))
            {
                await _sut.ContractCaller.ClaimThingAssessmentVerifierLotterySpotAs(privateKey, thingId);

                var committedDataHash = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes16(thingId))
                    .AsMapping()
                    .Key(new SolAddress(address))
                    .AsStruct("Commitment")
                    .Field("dataHash")
                    .GetValue<SolBytes32>();

                committedDataHash.Value.Should().Equal(new byte[32]);

                var revealed = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes16(thingId))
                    .AsMapping()
                    .Key(new SolAddress(address))
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

                await _sut.ContractCaller.PreJoinThingAssessmentVerifierLotteryAs(privateKey, thingId, dataHash);
                await _sut.ContractCaller.JoinThingAssessmentVerifierLotteryAs(privateKey, thingId, data);

                var committedDataHash = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes16(thingId))
                    .AsMapping()
                    .Key(new SolAddress(address))
                    .AsStruct("Commitment")
                    .Field("dataHash")
                    .GetValue<SolBytes32>();

                committedDataHash.Value.Should().Equal(dataHash);

                var revealed = await _thingAssessmentVerifierLotteryContract
                    .WalkStorage()
                    .Field("s_thingIdToLotteryCommitments")
                    .AsMapping()
                    .Key(new SolBytes16(thingId))
                    .AsMapping()
                    .Key(new SolAddress(address))
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

        await Task.Delay(TimeSpan.FromSeconds(20)); // giving time to close the lottery

        var orchestrator = _sut.GetConfigurationValue<string>($"Ethereum:Accounts:{network}:Orchestrator:Address");

        var block = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_thingIdToLotteryCommitments")
            .AsMapping()
            .Key(new SolBytes16(thingId))
            .AsMapping()
            .Key(new SolAddress(orchestrator))
            .AsStruct("Commitment")
            .Field("block")
            .GetValue<SolInt64>();

        block.Value.Should().Be(-1);
    }
}