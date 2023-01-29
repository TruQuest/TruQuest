using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;

using Infrastructure.Ethereum;
using Infrastructure.Ethereum.TypedData;

using Tests.FunctionalTests.Helpers.Messages;
using Tests.FunctionalTests.Helpers.Errors;

namespace Tests.FunctionalTests.Helpers;

public class ContractCaller
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly AccountProvider _accountProvider;
    private readonly BlockchainManipulator _blockchainManipulator;

    private readonly Web3 _web3;
    private readonly string _rpcUrl;
    private readonly string _truthserumAddress;
    private readonly string _truQuestAddress;
    private readonly string _thingSubmissionVerifierLotteryAddress;
    private readonly string _acceptancePollAddress;
    private readonly string _thingAssessmentVerifierLotteryAddress;
    private readonly string _assessmentPollAddress;

    private readonly Account _orchestrator;

    public ContractCaller(
        ILogger logger,
        IConfiguration configuration,
        AccountProvider accountProvider,
        BlockchainManipulator blockchainManipulator
    )
    {
        _logger = logger;
        _configuration = configuration;
        _accountProvider = accountProvider;
        _blockchainManipulator = blockchainManipulator;

        var network = configuration["Ethereum:Network"]!;
        var submitter = accountProvider.GetAccount("Submitter");
        _rpcUrl = configuration[$"Ethereum:Networks:{network}:URL"]!;
        _web3 = new Web3(submitter, _rpcUrl);
        _truthserumAddress = configuration[$"Ethereum:Contracts:{network}:Truthserum:Address"]!;
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
        _thingSubmissionVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
        _thingAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!;
        _assessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:AssessmentPoll:Address"]!;

        _orchestrator = accountProvider.GetAccount("Orchestrator");
    }

    public async Task FundThing(byte[] thingId, string signature)
    {
        signature = signature.Substring(2);
        var r = signature.Substring(0, 64).HexToByteArray();
        var s = signature.Substring(64, 64).HexToByteArray();
        var v = (byte)signature.Substring(128, 2).HexToBigInteger(true);

        var txnDispatcher = _web3.Eth.GetContractTransactionHandler<FundThingMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truQuestAddress,
            new()
            {
                Thing = new ThingTd
                {
                    Id = thingId
                },
                V = v,
                R = r,
                S = s
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task PreJoinThingSubmissionVerifierLotteryAs(string accountName, byte[] thingId, byte[] dataHash)
    {
        var account = _accountProvider.GetAccount(accountName);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<PreJoinThingSubmissionVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingSubmissionVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                DataHash = dataHash
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinThingSubmissionVerifierLotteryAs(string accountName, byte[] thingId, byte[] data)
    {
        var account = _accountProvider.GetAccount(accountName);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<JoinThingSubmissionVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingSubmissionVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                Data = data
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task FundThingSettlementProposal(byte[] thingId, byte[] proposalId, string signature)
    {
        var network = _configuration["Ethereum:Network"]!;
        var proposer = _accountProvider.GetAccount("Proposer");
        var web3 = new Web3(proposer, _rpcUrl);

        signature = signature.Substring(2);
        var r = signature.Substring(0, 64).HexToByteArray();
        var s = signature.Substring(64, 64).HexToByteArray();
        var v = (byte)signature.Substring(128, 2).HexToBigInteger(true);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<FundThingSettlementProposalMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truQuestAddress,
            new()
            {
                SettlementProposal = new SettlementProposalTd
                {
                    ThingId = thingId,
                    Id = proposalId
                },
                V = v,
                R = r,
                S = s
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task ClaimThingAssessmentVerifierLotterySpotAs(string accountName, byte[] thingProposalId)
    {
        var account = _accountProvider.GetAccount(accountName);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<ClaimLotterySpotMessage>();
        try
        {
            var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingProposalId = thingProposalId
                }
            );

        }
        catch (SmartContractCustomErrorRevertException ex)
        {
            if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__AlreadyCommittedToLotteryError>())
            {
                var error = ex.DecodeError<ThingAssessmentVerifierLottery__AlreadyCommittedToLotteryError>();
            }
            else if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__LotteryExpiredError>())
            {
                var error = ex.DecodeError<ThingAssessmentVerifierLottery__LotteryExpiredError>();
            }
            else if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__LotteryNotActiveError>())
            {
                var error = ex.DecodeError<ThingAssessmentVerifierLottery__LotteryNotActiveError>();
            }
            else if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__NotEnoughFundsError>())
            {
                var error = ex.DecodeError<ThingAssessmentVerifierLottery__NotEnoughFundsError>();
            }
        }

        await _blockchainManipulator.Mine(1);
    }

    public async Task PreJoinThingAssessmentVerifierLotteryAs(
        string accountName, byte[] thingProposalId, byte[] dataHash
    )
    {
        var account = _accountProvider.GetAccount(accountName);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<PreJoinThingAssessmentVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingAssessmentVerifierLotteryAddress,
            new()
            {
                ThingProposalId = thingProposalId,
                DataHash = dataHash
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinThingAssessmentVerifierLotteryAs(string accountName, byte[] thingProposalId, byte[] data)
    {
        var account = _accountProvider.GetAccount(accountName);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<JoinThingAssessmentVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingAssessmentVerifierLotteryAddress,
            new()
            {
                ThingProposalId = thingProposalId,
                Data = data
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task CastAssessmentPollVoteAs(string accountName, byte[] thingProposalId, Vote vote)
    {
        var account = _accountProvider.GetAccount(accountName);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<CastAssessmentPollVoteMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _assessmentPollAddress,
            new()
            {
                ThingProposalId = thingProposalId,
                Vote = vote
            }
        );

        await _blockchainManipulator.Mine(1);
    }
}