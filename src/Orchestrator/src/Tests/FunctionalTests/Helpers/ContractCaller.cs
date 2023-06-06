using System.Numerics;

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

    public async Task<long> GetAvailableFunds(string accountName)
    {
        var account = _accountProvider.GetAccount(accountName);
        var funds = await _web3.Eth
            .GetContractQueryHandler<GetAvailableFundsMessage>()
            .QueryAsync<BigInteger>(_truQuestAddress, new()
            {
                User = account.Address
            }
        );

        return (long)funds;
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

        _logger.LogInformation(
            "FundThing gas used: {CumulativeGas} {Gas}",
            txnReceipt.CumulativeGasUsed.Value,
            txnReceipt.GasUsed.Value
        ); // 83375

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

        _logger.LogInformation(
            "PreJoinThing gas used: {CumulativeGas} {Gas}",
            txnReceipt.CumulativeGasUsed.Value,
            txnReceipt.GasUsed.Value
        ); // 153762 first time, then 136662

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinThingSubmissionVerifierLotteryAs(string accountName, byte[] thingId, byte[] userData)
    {
        var account = _accountProvider.GetAccount(accountName);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<JoinThingSubmissionVerifierLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingSubmissionVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                UserData = userData
            }
        );

        _logger.LogInformation(
            "JoinThing gas used: {CumulativeGas} {Gas}",
            txnReceipt.CumulativeGasUsed.Value,
            txnReceipt.GasUsed.Value
        ); // 39180, but there is 39168 in the middle

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

        _logger.LogInformation(
            "FundPro gas used: {CumulativeGas} {Gas}",
            txnReceipt.CumulativeGasUsed.Value,
            txnReceipt.GasUsed.Value
        ); // 107233

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

            _logger.LogInformation(
                "ClaimSpot gas used: {CumulativeGas} {Gas}",
                txnReceipt.CumulativeGasUsed.Value,
                txnReceipt.GasUsed.Value
            ); // 135546 first time, then 118446
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

        _logger.LogInformation(
            "PreJoinPro gas used: {CumulativeGas} {Gas}",
            txnReceipt.CumulativeGasUsed.Value,
            txnReceipt.GasUsed.Value
        ); // 156558 first time, then 139458 x3, and 122358 last

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

        _logger.LogInformation(
            "JoinPro gas used: {CumulativeGas} {Gas}",
            txnReceipt.CumulativeGasUsed.Value,
            txnReceipt.GasUsed.Value
        ); // 39786, then 39774 once in the middle, and 39786 again

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

        _logger.LogInformation(
            "CastPro gas used: {CumulativeGas} {Gas}",
            txnReceipt.CumulativeGasUsed.Value,
            txnReceipt.GasUsed.Value
        ); // 36458 39075 41692 44309 46926

        await _blockchainManipulator.Mine(1);
    }
}