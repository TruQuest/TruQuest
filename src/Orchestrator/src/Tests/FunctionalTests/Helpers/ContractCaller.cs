using System.Numerics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;

using Application.Thing.Commands.SubmitNewThing;
using Application.Settlement.Commands.SubmitNewSettlementProposal;
using Infrastructure.Ethereum.TypedData;

using Tests.FunctionalTests.Helpers.Messages;
using Tests.FunctionalTests.Helpers.Errors;

namespace Tests.FunctionalTests.Helpers;

public class ContractCaller
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly BlockchainManipulator _blockchainManipulator;

    private readonly Web3 _web3;
    private readonly string _rpcUrl;
    private readonly string _truthserumAddress;
    private readonly string _truQuestAddress;
    private readonly string _thingSubmissionVerifierLotteryAddress;
    private readonly string _acceptancePollAddress;
    private readonly string _thingAssessmentVerifierLotteryAddress;

    private readonly Account _orchestrator;

    public ContractCaller(ILogger logger, IConfiguration configuration, BlockchainManipulator blockchainManipulator)
    {
        _logger = logger;
        _configuration = configuration;
        _blockchainManipulator = blockchainManipulator;

        var network = configuration["Ethereum:Network"]!;
        var submitter = new Account(configuration[$"Ethereum:Accounts:{network}:Submitter:PrivateKey"]);
        _rpcUrl = configuration[$"Ethereum:Networks:{network}:URL"]!;
        _web3 = new Web3(submitter, _rpcUrl);
        _truthserumAddress = configuration[$"Ethereum:Contracts:{network}:Truthserum:Address"]!;
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
        _thingSubmissionVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
        _thingAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!;

        _orchestrator = new Account(configuration[$"Ethereum:Accounts:{network}:Orchestrator:PrivateKey"]);
    }

    public async Task TransferTruthserumTo(string address, BigInteger amount)
    {
        var web3 = new Web3(_orchestrator, _rpcUrl);
        var txnDispatcher = web3.Eth.GetContractTransactionHandler<TransferMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truthserumAddress,
            new()
            {
                To = address,
                Amount = amount
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task DepositFundsAs(string privateKey, BigInteger amount)
    {
        var player = new Account(privateKey);
        var web3 = new Web3(player, _rpcUrl);

        var approveTxnDispatcher = web3.Eth.GetContractTransactionHandler<ApproveMessage>();
        var approveTxnReceipt = await approveTxnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truthserumAddress,
            new()
            {
                Spender = _truQuestAddress,
                Amount = amount
            }
        );

        await _blockchainManipulator.Mine(1);

        var depositTxnDispatcher = web3.Eth.GetContractTransactionHandler<DepositMessage>();
        var depositTxnReceipt = await depositTxnDispatcher.SendRequestAndWaitForReceiptAsync(
            _truQuestAddress,
            new()
            {
                Amount = amount
            }
        );

        await _blockchainManipulator.Mine(1);

        var balance = await web3.Eth.GetContractQueryHandler<GetAvailableFundsMessage>()
            .QueryAsync<BigInteger>(
                _truQuestAddress,
                new()
                {
                    User = player.Address
                }
            );

        _logger.LogInformation("User {Address} balance: {Balance}", player.Address, balance);
    }

    public async Task FundThing(ThingVm thing, string signature)
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
                    Id = thing.Id.ToByteArray()
                },
                V = v,
                R = r,
                S = s
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task PreJoinThingSubmissionVerifierLotteryAs(string privateKey, byte[] thingId, byte[] dataHash)
    {
        var account = new Account(privateKey);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<PreJoinLotteryMessage>();
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

    public async Task JoinThingSubmissionVerifierLotteryAs(string privateKey, byte[] thingId, byte[] data)
    {
        var account = new Account(privateKey);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<JoinLotteryMessage>();
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

    public async Task FundThingSettlementProposal(SettlementProposalVm proposal, string signature)
    {
        var network = _configuration["Ethereum:Network"]!;
        var proposer = new Account(_configuration[$"Ethereum:Accounts:{network}:Proposer:PrivateKey"]);
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
                    ThingId = proposal.ThingId.ToByteArray(),
                    Id = proposal.Id.ToByteArray()
                },
                V = v,
                R = r,
                S = s
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task ClaimThingAssessmentVerifierLotterySpotAs(string privateKey, byte[] thingId)
    {
        var account = new Account(privateKey);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<ClaimLotterySpotMessage>();
        try
        {
            var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
                _thingAssessmentVerifierLotteryAddress,
                new()
                {
                    ThingId = thingId
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

    public async Task PreJoinThingAssessmentVerifierLotteryAs(string privateKey, byte[] thingId, byte[] dataHash)
    {
        var account = new Account(privateKey);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<PreJoinLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingAssessmentVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                DataHash = dataHash
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinThingAssessmentVerifierLotteryAs(string privateKey, byte[] thingId, byte[] data)
    {
        var account = new Account(privateKey);
        var web3 = new Web3(account, _rpcUrl);

        var txnDispatcher = web3.Eth.GetContractTransactionHandler<JoinLotteryMessage>();
        var txnReceipt = await txnDispatcher.SendRequestAndWaitForReceiptAsync(
            _thingAssessmentVerifierLotteryAddress,
            new()
            {
                ThingId = thingId,
                Data = data
            }
        );

        await _blockchainManipulator.Mine(1);
    }
}