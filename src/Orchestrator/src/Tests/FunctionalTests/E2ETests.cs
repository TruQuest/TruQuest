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
using Application.Thing.Commands.CastValidationPollVote;
using Application.Settlement.Common.Models.IM;
using Application.Settlement.Commands.CreateNewSettlementProposalDraft;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Application.Thing.Queries.GetThing;
using Application.Common.Misc;
using Application.Common.Models.QM;
using Application.Settlement.Commands.CastAssessmentPollVote;
using ThingEthEvents = Application.Ethereum.Events.ThingValidationVerifierLottery;
using ProposalEthEvents = Application.Ethereum.Events.SettlementProposalAssessmentVerifierLottery;
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
    private Contract _thingValidationVerifierLotteryContract;
    private Contract _thingValidationPollContract;
    private Contract _settlementProposalAssessmentVerifierLotteryContract;
    private Contract _settlementProposalAssessmentPollContract;

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

        _settlementProposalAssessmentVerifierLotteryContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("SettlementProposalAssessmentVerifierLottery")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:SettlementProposalAssessmentVerifierLottery:Address"))
            .OnNetwork(rpcUrl)
            .Find();

        _settlementProposalAssessmentPollContract = ContractFinder.Create()
            .WithLayoutDirectory("c:/chekh/projects/truquest/src/dapp/contracts/layout")
            .WithName("SettlementProposalAssessmentPoll")
            .DeployedAt(_sut.GetConfigurationValue<string>($"Ethereum:Contracts:{network}:SettlementProposalAssessmentPoll:Address"))
            .OnNetwork(rpcUrl)
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
        var voteAgg = (await JsonSerializer.DeserializeAsync<SignedThingValidationPollVoteAggTd>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        ))!;

        var upperLimitTs = await _sut.ExecWithService<IL1BlockchainQueryable, long>(blockchainQueryable =>
            blockchainQueryable.GetBlockTimestamp((long)voteAgg.L1EndBlock)
        );

        var offChainVotes = await Task.WhenAll(voteAgg.OffChainVotes.Select(async v =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/ipfs/{v.IpfsCid}");
            using var response = await client.SendAsync(request);
            var signedVote = await JsonSerializer.DeserializeAsync<SignedNewThingValidationPollVoteTd>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var vote = NewThingValidationPollVoteIm.FromMessageForSigning(signedVote!.Vote);

            return (
                VoterId: signedVote.UserId,
                VoterWalletAddress: signedVote.WalletAddress,
                CastedAt: DateTimeOffset
                    .ParseExact(vote.CastedAt, "yyyy-MM-dd HH:mm:sszzz", null)
                    .ToUnixTimeMilliseconds(),
                Decision: vote.Decision
            );
        }));

        var onChainVotes = voteAgg.OnChainVotes.Select(v => (
            TxnHash: v.TxnHash,
            BlockNumber: v.BlockNumber,
            TxnIndex: v.TxnIndex,
            VoterId: v.UserId,
            VoterWalletAddress: v.WalletAddress,
            Decision: ThingValidationPollVoteDecisionExtension.FromString(v.Decision)
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
                VoterWalletAddress = onChainVote.VoterWalletAddress,
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
                VoterWalletAddress = offChainVote.VoterWalletAddress,
                VoteDecision = (AccountedVote.Decision)offChainVote.Decision
            });
        }

        return accountedVotes;
    }

    private async Task<(
        ValidationDecision Decision,
        IEnumerable<string> RewardedVerifiers,
        IEnumerable<string> PenalizedVerifiers
    )> _calculatePollResult(byte[] thingIdBytes, HashSet<AccountedVote> accountedVotes)
    {
        var verifierAddresses =
            (await _sut.ExecWithService<IContractCaller, IEnumerable<String>>(
                contractCaller => contractCaller.GetVerifiersForThing(thingIdBytes)
            ))
            .ToList();

        var notVotedVerifiers = verifierAddresses
            .Where(verifierAddress => accountedVotes.SingleOrDefault(v => v.VoterWalletAddress == verifierAddress) == null)
            .ToList();

        var votingVolumeThresholdPercent = await _sut.ExecWithService<IContractCaller, int>(
            contractCaller => contractCaller.GetThingValidationPollVotingVolumeThresholdPercent()
        );

        var requiredVoterCount = Math.Ceiling(votingVolumeThresholdPercent / 100f * verifierAddresses.Count);

        if (accountedVotes.Count < requiredVoterCount)
        {
            return (
                Decision: ValidationDecision.UnsettledDueToInsufficientVotingVolume,
                RewardedVerifiers: new string[] { },
                PenalizedVerifiers: notVotedVerifiers
            );
        }

        var majorityThresholdPercent = await _sut.ExecWithService<IContractCaller, int>(
            contractCaller => contractCaller.GetThingValidationPollMajorityThresholdPercent()
        );
        var acceptedDecisionRequiredVoteCount = Math.Ceiling(majorityThresholdPercent / 100f * accountedVotes.Count);

        var votesGroupedByDecision = accountedVotes.GroupBy(v => v.VoteDecision);
        var acceptedDecision = votesGroupedByDecision.MaxBy(group => group.Count())!;

        if (acceptedDecision.Count() < acceptedDecisionRequiredVoteCount)
        {
            return (
                Decision: ValidationDecision.UnsettledDueToMajorityThresholdNotReached,
                RewardedVerifiers: new string[] { },
                PenalizedVerifiers: notVotedVerifiers
            );
        }

        var verifiersThatDisagreedWithAcceptedDecisionDirection = votesGroupedByDecision
            .Where(v => v.Key.GetScore() != acceptedDecision.Key.GetScore())
            .SelectMany(v => v)
            .Select(v => v.VoterWalletAddress);

        var verifiersToSlash = notVotedVerifiers
            .Concat(verifiersThatDisagreedWithAcceptedDecisionDirection)
            .ToList();

        var verifiersToReward = votesGroupedByDecision
            .Where(v => v.Key.GetScore() == acceptedDecision.Key.GetScore())
            .SelectMany(v => v)
            .Select(v => v.VoterWalletAddress);

        if (acceptedDecision.Key == AccountedVote.Decision.Accept)
        {
            return (
                Decision: ValidationDecision.Accepted,
                RewardedVerifiers: verifiersToReward,
                PenalizedVerifiers: verifiersToSlash
            );
        }
        else if (acceptedDecision.Key == AccountedVote.Decision.SoftDecline)
        {
            return (
                Decision: ValidationDecision.SoftDeclined,
                RewardedVerifiers: verifiersToReward,
                PenalizedVerifiers: verifiersToSlash
            );
        }

        return (
            Decision: ValidationDecision.HardDeclined,
            RewardedVerifiers: verifiersToReward,
            PenalizedVerifiers: verifiersToSlash
        );
    }

    [Fact]
    public async Task ShouldCreateAndAcceptThingAndSubsequentSettlementProposal()
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
            await _sut.RunAs(accountName: "Submitter");

            var subjectResult = await _sut.SendRequest(new AddNewSubjectCommand(request.Request));

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
            var thingDraftResult = await _sut.SendRequest(new CreateNewThingDraftCommand(request.Request));

            thingId = thingDraftResult.Data;
        }

        await draftCreatedTcs.Task;

        var thingSubmitResult = await _sut.SendRequest(new SubmitNewThingCommand
        {
            ThingId = thingId
        });

        var thingStake = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingStake")
                .GetValue<SolUint256>()
        ).Value;

        Debug.WriteLine($"************ Thing stake: {thingStake} ************");

        var submitterInitialBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");

        Debug.WriteLine($"************ Submitter initial balance: {submitterInitialBalance} ************");

        var thingIdBytes = thingId.ToByteArray();

        var lotteryInitializedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingValidationVerifierLotteryInitialized += delegate
        {
            lotteryInitializedTcs.SetResult();
        };

        await _sut.ContractCaller.FundThingAs("Submitter", thingIdBytes, thingSubmitResult.Data!.Signature);

        var submitterBalance = await _sut.ContractCaller.GetAvailableFunds("Submitter");
        submitterBalance.Should().Be(submitterInitialBalance - thingStake);

        var submitter = await _truQuestContract
            .WalkStorage()
            .Field("s_thingSubmitter")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .GetValue<SolAddress>();

        var submitterAddress = await _sut.ContractCaller.GetWalletAddressFor("Submitter");

        submitter.Value.ToLower().Should().Be(submitterAddress.Substring(2).ToLower());

        await lotteryInitializedTcs.Task;

        var verifierStake = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_verifierStake")
                .GetValue<SolUint256>()
        ).Value;

        Debug.WriteLine($"************ Verifier stake: {verifierStake} ************");

        var verifierCount = 6;

        var cde = new AsyncCountdownEvent(verifierCount);
        _eventBroadcaster.JoinedThingValidationVerifierLottery += delegate
        {
            cde.Signal(1);
        };

        var thingLotteryClosedTcs = new TaskCompletionSource<ThingEthEvents.LotteryClosedWithSuccess.LotteryClosedWithSuccessEvent>();
        _eventBroadcaster.ThingValidationVerifierLotteryClosedWithSuccess += (_, @event) =>
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

        for (int i = 1; i <= verifierCount; ++i)
        {
            var verifierAccountName = $"Verifier{i}";

            var userData = RandomNumberGenerator.GetBytes(32);

            var verifierInitialBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);

            await _sut.ContractCaller.JoinThingValidationVerifierLotteryAs(verifierAccountName, thingIdBytes, userData);

            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);
            verifierBalance.Should().Be(verifierInitialBalance - verifierStake);

            var walletAddress = await _sut.ContractCaller.GetWalletAddressFor(verifierAccountName);

            verifiersLotteryData.Add((verifierAccountName, walletAddress, verifierInitialBalance, userData, null));
        }

        await cde.WaitAsync();

        var lotteryDurationBlocks = await _thingValidationVerifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks.Value);

        var thingLotteryClosedWithSuccessEvent = await thingLotteryClosedTcs.Task;

        var maxNonce = await _sut.ExecWithService<IContractCaller, BigInteger>(
            contractCaller => contractCaller.GetThingValidationVerifierLotteryMaxNonce()
        );

        var nonce = (long)(
            (
                new BigInteger(thingLotteryClosedWithSuccessEvent.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(thingLotteryClosedWithSuccessEvent.HashOfL1EndBlock, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        nonce.Should().Be(thingLotteryClosedWithSuccessEvent.Nonce);

        Debug.WriteLine(
            $"************** Server-computed nonce: {nonce}; Blockchain-computed nonce: {thingLotteryClosedWithSuccessEvent.Nonce} **************"
        );

        int requiredVerifierCount = await _sut.ExecWithService<IContractCaller, int>(
            contractCaller => contractCaller.GetThingValidationVerifierLotteryNumVerifiers()
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

        // giving time to add verifiers, change the thing's state, and create a validation poll closing task
        await Task.Delay(TimeSpan.FromSeconds(10));

        verifierCount = await _thingValidationPollContract
            .WalkStorage()
            .Field("s_thingVerifiers")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .AsArrayOf<SolAddress>()
            .Length();

        verifierCount.Should().Be(requiredVerifierCount);

        var thingValidationVerifierAccountNames = new List<string>();

        for (int i = 0; i < verifierCount; ++i)
        {
            var verifierAddress = (
                await _thingValidationPollContract
                    .WalkStorage()
                    .Field("s_thingVerifiers")
                    .AsMapping()
                    .Key(new SolBytes16(thingIdBytes))
                    .AsArrayOf<SolAddress>()
                    .Index(i)
                    .GetValue<SolAddress>()
            ).Value;

            var winnerData = winnersLotteryData.SingleOrDefault(d => d.WalletAddress.Substring(2).ToLower() == verifierAddress.ToLower());
            winnerData.Should().NotBeNull();

            var verifierAccountName = winnerData.AccountName;
            verifierAddress = winnerData.WalletAddress;

            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(verifierAccountName);
            verifierBalance.Should().Be(winnerData.InitialBalance - verifierStake);

            thingValidationVerifierAccountNames.Add(verifierAccountName);

            if (i % 2 == 0)
            {
                await _sut.RunAs(accountName: verifierAccountName);

                var voteInput = new NewThingValidationPollVoteIm
                {
                    ThingId = thingId,
                    CastedAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:sszzz"),
                    Decision = Application.Thing.Commands.CastValidationPollVote.DecisionIm.Accept,
                    Reason = "Some reason"
                };

                await _sut.SendRequest(new CastValidationPollVoteCommand
                {
                    Input = voteInput,
                    Signature = _sut.Signer.SignNewThingValidationPollVoteMessageAs(verifierAccountName, voteInput)
                });
            }
            else
            {
                await _sut.ContractCaller.CastThingValidationPollVoteAs(verifierAccountName, thingIdBytes, (ushort)i, Vote.Accept);
            }
        }

        var losersLotteryData = verifiersLotteryData.Skip(requiredVerifierCount);

        foreach (var loserData in losersLotteryData)
        {
            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(loserData.AccountName);
            verifierBalance.Should().Be(loserData.InitialBalance);
        }

        var pollFinalizedTcs = new TaskCompletionSource();
        _eventBroadcaster.ThingValidationPollFinalized += delegate
        {
            pollFinalizedTcs.SetResult();
        };

        var pollDurationBlocks = await _thingValidationPollContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(pollDurationBlocks.Value + 10);

        await pollFinalizedTcs.Task;

        await Task.Delay(TimeSpan.FromSeconds(10)); // giving time to update the thing's state and ipfs cid

        var thingAcceptedReward = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingAcceptedReward")
                .GetValue<SolUint256>()
        ).Value;
        Debug.WriteLine($"*************** Thing accepted reward: {thingAcceptedReward} ***************");

        var thingRejectedPenalty = (long)(
            await _truQuestContract
                .WalkStorage()
                .Field("s_thingRejectedPenalty")
                .GetValue<SolUint256>()
        ).Value;
        Debug.WriteLine(
            $"*************** Thing rejected penalty: {thingRejectedPenalty} ***************"
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
            ValidationDecision.UnsettledDueToInsufficientVotingVolume or
            ValidationDecision.UnsettledDueToMajorityThresholdNotReached
        )
        {
            thing.State.Should().Be(ThingStateQm.ConsensusNotReached);
            submitterBalance.Should().Be(submitterInitialBalance);
        }
        else if (pollResult.Decision is ValidationDecision.Accepted)
        {
            thing.State.Should().Be(ThingStateQm.AwaitingSettlement);
            submitterBalance.Should().Be(submitterInitialBalance + thingAcceptedReward);
        }
        else if (pollResult.Decision is ValidationDecision.SoftDeclined)
        {
            thing.State.Should().Be(ThingStateQm.Declined);
            submitterBalance.Should().Be(submitterInitialBalance);
        }
        else
        {
            thing.State.Should().Be(ThingStateQm.Declined);
            submitterBalance.Should().Be(submitterInitialBalance - thingRejectedPenalty);
        }

        (pollResult.RewardedVerifiers.Count() + pollResult.PenalizedVerifiers.Count()).Should().Be(verifierCount);

        foreach (var verifierAddress in pollResult.RewardedVerifiers)
        {
            var winnerData = winnersLotteryData.Single(d => d.WalletAddress == verifierAddress);
            var initialBalance = winnerData.InitialBalance;
            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(winnerData.AccountName);
            verifierBalance.Should().Be(initialBalance + verifierReward);
        }

        foreach (var verifierAddress in pollResult.PenalizedVerifiers)
        {
            var winnerData = winnersLotteryData.Single(d => d.WalletAddress == verifierAddress);
            var initialBalance = winnerData.InitialBalance;
            var verifierBalance = await _sut.ContractCaller.GetAvailableFunds(winnerData.AccountName);
            verifierBalance.Should().Be(initialBalance - verifierPenalty);
        }

        draftCreatedTcs = new TaskCompletionSource();
        _eventBroadcaster.ProposalDraftCreated += delegate
        {
            draftCreatedTcs.SetResult();
        };

        Guid proposalId;
        using (var request = _sut.PrepareHttpRequestForFileUpload(
            fileNames: new[] { "full-image.jpg", "cropped-image-rect.png" },
            ("thingId", thingId.ToString()),
            ("title", "Go to the Moooooon..."),
            ("verdict", $"{(int)VerdictIm.NoEffortWhatsoever}"),
            ("details", _dummyQuillContentJson),
            ("evidence", "https://google.com")
        ))
        {
            await _sut.RunAs(accountName: "Proposer");

            var proposalDraftResult = await _sut.SendRequest(
                new CreateNewSettlementProposalDraftCommand(request.Request)
            );

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

        await _sut.ContractCaller.FundSettlementProposalAs(
            "Proposer", thingIdBytes, proposalIdBytes, proposalSubmitResult.Data!.Signature
        );

        var proposer = await _truQuestContract
            .WalkStorage()
            .Field("s_thingIdToSettlementProposal")
            .AsMapping()
            .Key(new SolBytes16(thingIdBytes))
            .AsStruct("SettlementProposal")
            .Field("submitter")
            .GetValue<SolAddress>();

        var proposerAddress = await _sut.ContractCaller.GetWalletAddressFor("Proposer");

        proposer.Value.ToLower().Should().Be(proposerAddress.Substring(2).ToLower());

        await lotteryInitializedTcs.Task;

        verifierCount = 6;

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

        var proposalVerifiersLotteryData = new List<(string AccountName, string WalletAddress)>();

        var thingProposalIdBytes = thingIdBytes.Concat(proposalIdBytes).ToArray();

        for (int i = 1; i <= verifierCount; ++i)
        {
            var verifierAccountName = $"Verifier{i}";
            var walletAddress = await _sut.ContractCaller.GetWalletAddressFor(verifierAccountName);

            if (thingValidationVerifierAccountNames.Contains(verifierAccountName))
            {
                var index = await _sut.ContractCaller.GetUserIndexAmongThingVerifiers(thingIdBytes, verifierAccountName);
                index.Should().BeGreaterThanOrEqualTo(0);

                await _sut.ContractCaller.ClaimSettlementProposalAssessmentVerifierLotterySpotAs(
                    verifierAccountName, thingProposalIdBytes, (ushort)index
                );
            }
            else
            {
                var userData = RandomNumberGenerator.GetBytes(32);
                await _sut.ContractCaller.JoinSettlementProposalAssessmentVerifierLotteryAs(
                    verifierAccountName, thingProposalIdBytes, userData
                );
            }

            proposalVerifiersLotteryData.Add((verifierAccountName, walletAddress));
        }

        await cde.WaitAsync();

        lotteryDurationBlocks = await _settlementProposalAssessmentVerifierLotteryContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(lotteryDurationBlocks.Value);

        var proposalLotteryClosedWithSuccessEvent = await proposalLotteryClosedTcs.Task;

        maxNonce = await _sut.ExecWithService<IContractCaller, BigInteger>(
            contractCaller => contractCaller.GetSettlementProposalAssessmentVerifierLotteryMaxNonce()
        );

        nonce = (long)(
            (
                new BigInteger(proposalLotteryClosedWithSuccessEvent.Data, isUnsigned: true, isBigEndian: true) ^
                new BigInteger(proposalLotteryClosedWithSuccessEvent.HashOfL1EndBlock, isUnsigned: true, isBigEndian: true)
            ) % maxNonce
        );

        nonce.Should().Be(proposalLotteryClosedWithSuccessEvent.Nonce);

        // giving time to add verifiers, change the proposal's state, and create an assessment poll closing task
        await Task.Delay(TimeSpan.FromSeconds(10));

        requiredVerifierCount = await _sut.ExecWithService<IContractCaller, int>(
            contractCaller => contractCaller.GetSettlementProposalAssessmentVerifierLotteryNumVerifiers()
        );

        verifierCount = await _settlementProposalAssessmentPollContract
            .WalkStorage()
            .Field("s_settlementProposalVerifiers")
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
            var verifierAddress = (
                await _settlementProposalAssessmentPollContract
                    .WalkStorage()
                    .Field("s_settlementProposalVerifiers")
                    .AsMapping()
                    .Key(new SolBytes32(thingProposalIdBytes))
                    .AsArrayOf<SolAddress>()
                    .Index(i)
                    .GetValue<SolAddress>()
            ).Value;

            var verifierAccountName = proposalVerifiersLotteryData
                .Single(d => d.WalletAddress.Substring(2).ToLower() == verifierAddress.ToLower())
                .AccountName;

            if (i % 2 == 0)
            {
                await _sut.RunAs(accountName: verifierAccountName);

                var voteInput = new NewSettlementProposalAssessmentPollVoteIm
                {
                    ThingId = thingId,
                    SettlementProposalId = proposalId,
                    CastedAt = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:sszzz"),
                    Decision = Application.Settlement.Commands.CastAssessmentPollVote.DecisionIm.Accept,
                    Reason = "Some reason"
                };

                await _sut.SendRequest(new CastAssessmentPollVoteCommand
                {
                    Input = voteInput,
                    Signature = _sut.Signer.SignNewSettlementProposalAssessmentPollVoteMessageAs(verifierAccountName, voteInput)
                });
            }
            else
            {
                await _sut.ContractCaller.CastSettlementProposalAssessmentPollVoteAs(
                    verifierAccountName, thingProposalIdBytes, (ushort)i, Vote.Accept
                );
            }
        }

        await cde.WaitAsync();

        pollDurationBlocks = await _settlementProposalAssessmentPollContract
            .WalkStorage()
            .Field("s_durationBlocks")
            .GetValue<SolUint16>();

        await _sut.BlockchainManipulator.Mine(pollDurationBlocks.Value);

        await pollFinalizedTcs.Task;

        await Task.Delay(TimeSpan.FromSeconds(10)); // giving time to update the proposal and thing's states and the like
    }
}
