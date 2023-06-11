using System.Numerics;
using System.Text.Json;
using System.Diagnostics;
using System.Security.Cryptography;

using Nito.AsyncEx;
using FluentAssertions;
using ContractStorageExplorer;
using ContractStorageExplorer.SolTypes;

using Domain.Aggregates;
using Application.Common.Interfaces;
using Application.Subject.Commands.AddNewSubject;
using Application.Thing.Commands.CreateNewThingDraft;
using Application.Thing.Commands.SubmitNewThing;
using Application.Thing.Commands.CastAcceptancePollVote;
using Application.Settlement.Common.Models.IM;
using Application.Settlement.Commands.CreateNewSettlementProposalDraft;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Application.Thing.Queries.GetThing;
using Application.Common.Misc;
using Application.Common.Models.QM;
using ThingEthEvents = Application.Ethereum.Events.ThingSubmissionVerifierLottery;
using ProposalEthEvents = Application.Ethereum.Events.ThingAssessmentVerifierLottery;
using Infrastructure.Ethereum.TypedData;
using API.BackgroundServices;

using Tests.FunctionalTests.Helpers;
using Tests.FunctionalTests.Helpers.Messages;

namespace Tests.FunctionalTests;

[Collection(nameof(TruQuestTestCollection))]
public class E2ETests : IAsyncLifetime
{
    private readonly Sut _sut;
    private EventBroadcaster _eventBroadcaster;

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

    public E2ETests(Sut sut)
    {
        _sut = sut;
        _dummyQuillContentJson = JsonSerializer.Serialize(_dummyQuillContent);
    }

    public async Task InitializeAsync()
    {
        await _sut.ResetState();

        _eventBroadcaster = new EventBroadcaster(_sut.ContractEventSink.Stream);
        _eventBroadcaster.Start();

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
        _eventBroadcaster.Stop();
    }

    private async Task<HashSet<AccountedVote>> _getVotes(ThingQm thing)
    {
        var ipfsGatewayHost = _sut.GetConfigurationValue<string>("IPFS:GatewayHost");
        using var client = new HttpClient();
        client.BaseAddress = new Uri(ipfsGatewayHost);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/ipfs/{thing.VoteAggIpfsCid!}");
        using var response = await client.SendAsync(request);
        var voteAgg = (await JsonSerializer.DeserializeAsync<SignedAcceptancePollVoteAggTd>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ))!;

        var upperLimitTs = await _sut.ExecWithService<IL1BlockchainQueryable, long>(blockchainQueryable =>
            blockchainQueryable.GetBlockTimestamp((long)voteAgg.EndBlock)
        );

        var offChainVotes = await Task.WhenAll(voteAgg.OffChainVotes.Select(async v =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/ipfs/{v.IpfsCid}");
            using var response = await client.SendAsync(request);
            var vote = await JsonSerializer.DeserializeAsync<SignedNewAcceptancePollVoteTd>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return (
                VoterId: vote!.VoterId,
                CastedAt: DateTimeOffset
                    .ParseExact(vote.Vote.CastedAt, "yyyy-MM-dd HH:mm:sszzz", null)
                    .ToUniversalTime()
                    .ToUnixTimeMilliseconds(),
                Decision: DecisionImExtension.FromString(vote.Vote.Decision)
            );
        }));

        var onChainVotes = voteAgg.OnChainVotes.Select(v => (
            VoterId: v.UserId,
            BlockNumber: v.BlockNumber,
            TxnIndex: v.TxnIndex,
            Decision: AcceptancePollVoteDecisionExtension.FromString(v.Decision)
        ));

        var accountedVotes = new HashSet<AccountedVote>();
        foreach (var onChainVote in
            onChainVotes
                .OrderByDescending(e => e.BlockNumber)
                    .ThenByDescending(e => e.TxnIndex)
        )
        {
            accountedVotes.Add(new()
            {
                VoterId = onChainVote.VoterId,
                VoteDecision = (AccountedVote.Decision)onChainVote.Decision
            });
        }
        foreach (var offChainVote in
            offChainVotes
                .Where(v => v.CastedAt <= upperLimitTs)
                .OrderByDescending(v => v.CastedAt)
        )
        {
            accountedVotes.Add(new()
            {
                VoterId = offChainVote.VoterId,
                VoteDecision = (AccountedVote.Decision)offChainVote.Decision
            });
        }

        return accountedVotes;
    }

    private async Task<(
        SubmissionEvaluationDecision Decision,
        IEnumerable<string> RewardedVerifiers,
        IEnumerable<string> PenalizedVerifiers
    )> _calculatePollResult(byte[] thingIdBytes, HashSet<AccountedVote> accountedVotes)
    {
        var verifiers = (await _sut.ExecWithService<IContractCaller, IEnumerable<String>>(
            contractCaller => contractCaller.GetVerifiersForThing(thingIdBytes)
        ))
        .Select(v => v.Substring(2).ToLower())
        .ToList();

        var notVotedVerifiers = verifiers
            .Where(verifierId => accountedVotes.SingleOrDefault(v => v.VoterId == verifierId) == null)
            .ToList();

        var votingVolumeThresholdPercent = await _sut.ExecWithService<IContractStorageQueryable, int>(
            contractStorageQueryable => contractStorageQueryable.GetThingAcceptancePollVotingVolumeThreshold()
        );

        var requiredVoterCount = Math.Ceiling(votingVolumeThresholdPercent / 100f * verifiers.Count);

        if (accountedVotes.Count < requiredVoterCount)
        {
            return (
                Decision: SubmissionEvaluationDecision.UnsettledDueToInsufficientVotingVolume,
                RewardedVerifiers: new string[] { },
                PenalizedVerifiers: notVotedVerifiers
            );
        }

        var majorityThresholdPercent = await _sut.ExecWithService<IContractStorageQueryable, int>(
            contractStorageQueryable => contractStorageQueryable.GetThingAcceptancePollMajorityThreshold()
        );
        var acceptedDecisionRequiredVoteCount = Math.Ceiling(majorityThresholdPercent / 100f * accountedVotes.Count);

        var votesGroupedByDecision = accountedVotes.GroupBy(v => v.VoteDecision);
        var acceptedDecision = votesGroupedByDecision.MaxBy(group => group.Count())!;

        if (acceptedDecision.Count() < acceptedDecisionRequiredVoteCount)
        {
            return (
                Decision: SubmissionEvaluationDecision.UnsettledDueToMajorityThresholdNotReached,
                RewardedVerifiers: new string[] { },
                PenalizedVerifiers: notVotedVerifiers
            );
        }

        var verifiersThatDisagreedWithAcceptedDecisionDirection = votesGroupedByDecision
            .Where(v => v.Key.GetScore() != acceptedDecision.Key.GetScore())
            .SelectMany(v => v)
            .Select(v => v.VoterId);

        var verifiersToSlash = notVotedVerifiers
            .Concat(verifiersThatDisagreedWithAcceptedDecisionDirection)
            .ToList();

        var verifiersToReward = votesGroupedByDecision
            .Where(v => v.Key.GetScore() == acceptedDecision.Key.GetScore())
            .SelectMany(v => v)
            .Select(v => v.VoterId);

        if (acceptedDecision.Key == AccountedVote.Decision.Accept)
        {
            return (
                Decision: SubmissionEvaluationDecision.Accepted,
                RewardedVerifiers: verifiersToReward,
                PenalizedVerifiers: verifiersToSlash
            );
        }
        else if (acceptedDecision.Key == AccountedVote.Decision.SoftDecline)
        {
            return (
                Decision: SubmissionEvaluationDecision.SoftDeclined,
                RewardedVerifiers: verifiersToReward,
                PenalizedVerifiers: verifiersToSlash
            );
        }

        return (
            Decision: SubmissionEvaluationDecision.HardDeclined,
            RewardedVerifiers: verifiersToReward,
            PenalizedVerifiers: verifiersToSlash
        );
    }

    [Fact]
    public async Task ShouldDoStuff()
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
            _sut.RunAs(userId: submitterAddress, username: submitterAddress.Substring(0, 20));

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

        await draftCreatedTcs.Task;

        var thingSubmitResult = await _sut.SendRequest(new SubmitNewThingCommand
        {
            ThingId = thingId
        });

        var thingSubmissionStake = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingSubmissionStake")
                .GetValue<SolUint256>()
        ).Value;

        Debug.WriteLine($"************ Thing submission stake: {thingSubmissionStake} ************");

        var submitterInitialBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");

        Debug.WriteLine($"************ Submitter initial balance: {submitterInitialBalance} ************");

        var thingIdBytes = thingId.ToByteArray();

        var lotteryInitializedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingSubmissionVerifierLotteryInitialized += delegate
        {
            lotteryInitializedTcs.SetResult();
        };

        await _sut.ContractCaller.FundThing(thingIdBytes, thingSubmitResult.Data!.Signature);

        var submitterBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");
        submitterBalance.Should().Be(submitterInitialBalance - thingSubmissionStake);

        var submitter = await _truQuestContract
            .WalkStorage()
            .Field("s_thingSubmitter")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .GetValue<SolAddress>();

        submitter.Value.ToLower().Should().Be(submitterAddress);

        await lotteryInitializedTcs.Task;

        var verifierStake = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_verifierStake")
                .GetValue<SolUint256>()
        ).Value;

        Debug.WriteLine($"************ Verifier stake: {verifierStake} ************");

        var verifierCount = 10;

        var cde = new AsyncCountdownEvent(verifierCount);
        _eventBroadcaster.JoinedThingSubmissionVerifierLottery += delegate
        {
            cde.Signal(1);
        };

        var thingLotteryClosedTcs = new TaskCompletionSource<ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent>();
        _eventBroadcaster.ThingSubmissionVerifierLotteryClosedWithSuccess += (_, @event) =>
        {
            thingLotteryClosedTcs.SetResult(@event.Event);
        };

        var verifiersLotteryData = new List<(string AccountName, long InitialBalance, byte[] UserData, long? Nonce)>();

        for (int i = 1; i <= verifierCount; ++i)
        {
            var verifierAccountName = $"Verifier{i}";
            var verifier = _sut.AccountProvider.GetAccount(verifierAccountName);

            var userData = RandomNumberGenerator.GetBytes(32);

            var verifierInitialBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);

            await _sut.ContractCaller.JoinThingSubmissionVerifierLotteryAs(verifierAccountName, thingIdBytes, userData);

            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);

            verifierBalance.Should().Be(verifierInitialBalance - verifierStake);

            verifiersLotteryData.Add((verifierAccountName, verifierInitialBalance, userData, null));
        }

        await cde.WaitAsync();

        var lotteryDurationBlocks = await _thingSubmissionVerifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks.Value);

        var thingLotteryClosedWithSuccessEvent = await thingLotteryClosedTcs.Task;

        var maxNonce = await _sut.ExecWithService<IContractCaller, BigInteger>(
            contractCaller => contractCaller.GetThingSubmissionVerifierLotteryMaxNonce()
        );

        var nonce = (long)(
            (
                new BigInteger(thingLotteryClosedWithSuccessEvent.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(thingLotteryClosedWithSuccessEvent.HashOfL1EndBlock, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        nonce.Should().Be(thingLotteryClosedWithSuccessEvent.Nonce);

        Debug.WriteLine($"Server-computed nonce: {nonce}; Blockchain-computed nonce: {thingLotteryClosedWithSuccessEvent.Nonce}");

        int requiredVerifierCount = await _sut.ExecWithService<IContractStorageQueryable, int>(
            contractStorageQueryable => contractStorageQueryable.GetThingSubmissionNumVerifiers()
        );

        var userXorData = thingLotteryClosedWithSuccessEvent.UserXorData;

        verifiersLotteryData = verifiersLotteryData
            .Select((d, i) =>
            {
                d.Nonce = (long)(
                    (
                        new BigInteger(d.UserData, isUnsigned: true, isBigEndian: true) ^
                        new BigInteger(userXorData, isUnsigned: true, isBigEndian: true)
                    ) % maxNonce
                );
                return (Index: i, Data: d);
            })
            .OrderBy(d => Math.Abs(nonce - d.Data.Nonce!.Value))
                .ThenBy(d => d.Index)
            .Select(d => d.Data)
            .ToList();

        var winnersLotteryData = verifiersLotteryData
            .Take(requiredVerifierCount)
            .ToList();

        await _sut.BlockchainManipulator.Mine(1);
        // giving time to add verifiers, change the thing's state, and create an acceptance poll closing task;
        // or to archive the thing
        await Task.Delay(TimeSpan.FromSeconds(10));

        verifierCount = await _acceptancePollContract
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

            var winnerData = winnersLotteryData.SingleOrDefault(v => v.AccountName == verifierAccountName);
            winnerData.Should().NotBeNull();

            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);
            verifierBalance.Should().Be(winnerData.InitialBalance - verifierStake);

            thingSubmissionVerifierAccountNames.Add(verifierAccountName);

            _sut.RunAs(userId: verifier.Value.ToLower(), username: verifier.Value.ToLower().Substring(0, 20));

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

        var losersLotteryData = verifiersLotteryData.Skip(requiredVerifierCount);

        foreach (var loserData in losersLotteryData)
        {
            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(loserData.AccountName);
            verifierBalance.Should().Be(loserData.InitialBalance);
        }

        var pollFinalizedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingAcceptancePollFinalized += delegate
        {
            pollFinalizedTcs.SetResult();
        };

        var pollDurationBlocks = await _acceptancePollContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(pollDurationBlocks.Value + 10);

        await pollFinalizedTcs.Task;

        await _sut.BlockchainManipulator.Mine(1);

        await Task.Delay(TimeSpan.FromSeconds(10)); // giving time to update the thing's state and ipfs cid

        var thingSubmissionAcceptedReward = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingSubmissionAcceptedReward")
                .GetValue<SolUint256>()
        ).Value;
        Debug.WriteLine($"*************** Thing submission accepted reward: {thingSubmissionAcceptedReward} ***************");

        var thingSubmissionRejectedPenalty = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingSubmissionRejectedPenalty")
                .GetValue<SolUint256>()
        ).Value;
        Debug.WriteLine(
            $"*************** Thing submission rejected penalty: {thingSubmissionRejectedPenalty} ***************"
        );

        var verifierReward = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_verifierReward")
                .GetValue<SolUint256>()
        ).Value;
        Debug.WriteLine($"*************** Verifier reward: {verifierReward} ***************");

        var verifierPenalty = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_verifierPenalty")
                .GetValue<SolUint256>()
        ).Value;
        Debug.WriteLine($"*************** Verifier penalty: {verifierPenalty} ***************");

        var thingResult = await _sut.SendRequest(new GetThingQuery { ThingId = thingId });
        var thing = thingResult.Data!.Thing;

        thing.VoteAggIpfsCid.Should().NotBeNull();

        var accountedVotes = await _getVotes(thing);
        var pollResult = await _calculatePollResult(thingIdBytes, accountedVotes);

        submitterBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");

        if (pollResult.Decision is
            SubmissionEvaluationDecision.UnsettledDueToInsufficientVotingVolume or
            SubmissionEvaluationDecision.UnsettledDueToMajorityThresholdNotReached
        )
        {
            thing.State.Should().Be(ThingStateQm.ConsensusNotReached);
            submitterBalance.Should().Be(submitterInitialBalance);
        }
        else if (pollResult.Decision is SubmissionEvaluationDecision.Accepted)
        {
            thing.State.Should().Be(ThingStateQm.AwaitingSettlement);
            submitterBalance.Should().Be(submitterInitialBalance + thingSubmissionAcceptedReward);
        }
        else if (pollResult.Decision is SubmissionEvaluationDecision.SoftDeclined)
        {
            thing.State.Should().Be(ThingStateQm.Declined);
            submitterBalance.Should().Be(submitterInitialBalance);
        }
        else
        {
            thing.State.Should().Be(ThingStateQm.Declined);
            submitterBalance.Should().Be(submitterInitialBalance - thingSubmissionRejectedPenalty);
        }

        (pollResult.RewardedVerifiers.Count() + pollResult.PenalizedVerifiers.Count()).Should().Be(verifierCount);

        foreach (var verifier in pollResult.RewardedVerifiers)
        {
            var verifierAccountName = _sut.AccountProvider.LookupNameByAddress(verifier);
            var initialBalance = winnersLotteryData.Single(w => w.AccountName == verifierAccountName).InitialBalance;
            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);
            verifierBalance.Should().Be(initialBalance + verifierReward);
        }

        foreach (var verifier in pollResult.PenalizedVerifiers)
        {
            var verifierAccountName = _sut.AccountProvider.LookupNameByAddress(verifier);
            var initialBalance = winnersLotteryData.Single(w => w.AccountName == verifierAccountName).InitialBalance;
            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);
            verifierBalance.Should().Be(initialBalance - verifierPenalty);
        }

        draftCreatedTcs = new TaskCompletionSource();
        _eventBroadcaster.ProposalDraftCreated += delegate
        {
            draftCreatedTcs.SetResult();
        };

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
            _sut.RunAs(userId: proposerAddress, username: proposerAddress.Substring(0, 20));

            var proposalDraftResult = await _sut.SendRequest(new CreateNewSettlementProposalDraftCommand
            {
                Request = request.Request
            });

            proposalId = proposalDraftResult.Data;
        }

        await draftCreatedTcs.Task;

        var proposalSubmitResult = await _sut.SendRequest(new SubmitNewSettlementProposalCommand
        {
            ProposalId = proposalId
        });

        lotteryInitializedTcs = new TaskCompletionSource();
        _eventBroadcaster.ProposalAssessmentVerifierLotteryInitialized += delegate
        {
            lotteryInitializedTcs.SetResult();
        };

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

        await lotteryInitializedTcs.Task;

        cde = new AsyncCountdownEvent(verifierCount);
        _eventBroadcaster.ClaimedProposalAssessmentVerifierLotterySpot += delegate
        {
            cde.Signal(1);
        };
        _eventBroadcaster.JoinedProposalAssessmentVerifierLottery += delegate
        {
            cde.Signal(1);
        };

        var proposalLotteryClosedTcs = new TaskCompletionSource<ProposalEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent>();
        _eventBroadcaster.ProposalAssessmentVerifierLotteryClosedWithSuccess += (_, @event) =>
        {
            proposalLotteryClosedTcs.SetResult(@event.Event);
        };

        var thingProposalIdBytes = thingIdBytes.Concat(proposalIdBytes).ToArray();

        for (int i = 1; i <= verifierCount; ++i)
        {
            var verifierAccountName = $"Verifier{i}";
            var verifier = _sut.AccountProvider.GetAccount(verifierAccountName);

            if (thingSubmissionVerifierAccountNames.Contains(verifierAccountName))
            {
                var index = await _sut.ContractCaller.GetUserIndexAmongThingVerifiers(thingIdBytes, verifierAccountName);
                index.Should().BeGreaterThanOrEqualTo(0);

                await _sut.ContractCaller.ClaimThingAssessmentVerifierLotterySpotAs(
                    verifierAccountName, thingProposalIdBytes, (ushort)index
                );
            }
            else
            {
                var userData = RandomNumberGenerator.GetBytes(32);
                await _sut.ContractCaller.JoinThingAssessmentVerifierLotteryAs(
                    verifierAccountName, thingProposalIdBytes, userData
                );
            }
        }

        await cde.WaitAsync();

        lotteryDurationBlocks = await _thingAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks.Value);

        var proposalLotteryClosedWithSuccessEvent = await proposalLotteryClosedTcs.Task;

        maxNonce = await _sut.ExecWithService<IContractCaller, BigInteger>(
            contractCaller => contractCaller.GetThingAssessmentVerifierLotteryMaxNonce()
        );

        nonce = (long)(
            (
                new BigInteger(proposalLotteryClosedWithSuccessEvent.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(proposalLotteryClosedWithSuccessEvent.HashOfL1EndBlock, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        nonce.Should().Be(proposalLotteryClosedWithSuccessEvent.Nonce);

        await _sut.BlockchainManipulator.Mine(1);
        // giving time to add verifiers, change the proposal's state, and create an assessment poll closing task
        await Task.Delay(TimeSpan.FromSeconds(10));

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

        cde = new AsyncCountdownEvent(verifierCount);
        _eventBroadcaster.CastedProposalAssessmentVote += delegate
        {
            cde.Signal(1);
        };

        pollFinalizedTcs = new TaskCompletionSource();
        _eventBroadcaster.ProposalAssessmentPollFinalized += delegate
        {
            pollFinalizedTcs.SetResult();
        };

        for (int i = 0; i < verifierCount; ++i)
        {
            var verifier = await _assessmentPollContract
                .WalkStorage()
                .Field("s_proposalVerifiers")
                .AsMapping()
                .Key(new SolBytes32(thingProposalIdBytes))
                .AsArrayOf<SolAddress>()
                .Index(i)
                .GetValue<SolAddress>();

            var verifierAccountName = _sut.AccountProvider.LookupNameByAddress(verifier.Value);

            await _sut.ContractCaller.CastAssessmentPollVoteAs(
                verifierAccountName, thingProposalIdBytes, (ushort)i, Vote.Accept
            );
        }

        await cde.WaitAsync();

        pollDurationBlocks = await _assessmentPollContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(pollDurationBlocks.Value);

        await pollFinalizedTcs.Task;

        await _sut.BlockchainManipulator.Mine(1);

        await Task.Delay(TimeSpan.FromSeconds(10)); // giving time to update the proposal and thing's states and the like
    }
}