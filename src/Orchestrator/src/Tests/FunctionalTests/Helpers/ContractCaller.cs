using System.Numerics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;

using Infrastructure.Ethereum;
using Infrastructure.Ethereum.Messages;

using Tests.FunctionalTests.Helpers.Messages;

namespace Tests.FunctionalTests.Helpers;

public class ContractCaller
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IAccountProvider _accountProvider;
    private readonly UserOperationService _userOperationService;
    private readonly BlockchainManipulator _blockchainManipulator;

    private readonly Web3 _web3;
    private readonly string _rpcUrl;
    private readonly string _simpleAccountFactoryAddress;
    private readonly string _truthserumAddress;
    private readonly string _truQuestAddress;
    private readonly string _thingValidationVerifierLotteryAddress;
    private readonly string _thingValidationPollAddress;
    private readonly string _settlementProposalAssessmentVerifierLotteryAddress;
    private readonly string _settlementProposalAssessmentPollAddress;

    public ContractCaller(
        ILogger logger,
        IConfiguration configuration,
        IAccountProvider accountProvider,
        UserOperationService userOperationService,
        BlockchainManipulator blockchainManipulator
    )
    {
        _logger = logger;
        _configuration = configuration;
        _accountProvider = accountProvider;
        _userOperationService = userOperationService;
        _blockchainManipulator = blockchainManipulator;

        var network = configuration["Ethereum:Network"]!;
        _web3 = new Web3(configuration[$"Ethereum:Networks:{network}:URL"]!);
        _simpleAccountFactoryAddress = configuration[$"Ethereum:Contracts:{network}:SimpleAccountFactory:Address"]!;
        _truthserumAddress = configuration[$"Ethereum:Contracts:{network}:Truthserum:Address"]!;
        _truQuestAddress = configuration[$"Ethereum:Contracts:{network}:TruQuest:Address"]!;
        _thingValidationVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationVerifierLottery:Address"]!;
        _thingValidationPollAddress = configuration[$"Ethereum:Contracts:{network}:ThingValidationPoll:Address"]!;
        _settlementProposalAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentVerifierLottery:Address"]!;
        _settlementProposalAssessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:SettlementProposalAssessmentPoll:Address"]!;
    }

    public Task<string> GetWalletAddressFor(string accountName) => _web3.Eth
        .GetContractQueryHandler<GetAddressMessage>()
        .QueryAsync<string>(
            _simpleAccountFactoryAddress,
            new()
            {
                Owner = _accountProvider.GetAccount(accountName).Address,
                Salt = 0
            }
        );

    public async Task<long> GetAvailableFunds(string accountName)
    {
        var funds = await _web3.Eth
            .GetContractQueryHandler<GetAvailableFundsMessage>()
            .QueryAsync<BigInteger>(
                _truQuestAddress,
                new()
                {
                    User = await GetWalletAddressFor(accountName)
                }
            );

        return (long)funds;
    }

    public async Task FundThingAs(string accountName, byte[] thingId, string signature)
    {
        signature = signature.Substring(2);
        var r = signature.Substring(0, 64).HexToByteArray();
        var s = signature.Substring(64, 64).HexToByteArray();
        var v = (byte)signature.Substring(128, 2).HexToBigInteger(true);

        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _truQuestAddress,
            new FundThingMessage
            {
                ThingId = thingId,
                V = v,
                R = r,
                S = s
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinThingValidationVerifierLotteryAs(string accountName, byte[] thingId, byte[] userData)
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _thingValidationVerifierLotteryAddress,
            message: new JoinThingValidationVerifierLotteryMessage
            {
                ThingId = thingId,
                UserData = userData
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task CastThingValidationPollVoteAs(
        string accountName, byte[] thingId, ushort thingVerifiersArrayIndex, Vote vote
    )
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _thingValidationPollAddress,
            message: new CastThingValidationPollVoteMessage
            {
                ThingId = thingId,
                ThingVerifiersArrayIndex = thingVerifiersArrayIndex,
                Vote = vote
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task FundSettlementProposalAs(
        string accountName, byte[] thingId, byte[] proposalId, string signature
    )
    {
        signature = signature.Substring(2);
        var r = signature.Substring(0, 64).HexToByteArray();
        var s = signature.Substring(64, 64).HexToByteArray();
        var v = (byte)signature.Substring(128, 2).HexToBigInteger(true);

        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _truQuestAddress,
            message: new FundSettlementProposalMessage
            {
                ThingId = thingId,
                ProposalId = proposalId,
                V = v,
                R = r,
                S = s
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task<int> GetUserIndexAmongThingVerifiers(byte[] thingId, string accountName)
    {
        var index = await _web3.Eth
            .GetContractQueryHandler<GetUserIndexAmongThingVerifiersMessage>()
            .QueryAsync<BigInteger>(
                _thingValidationPollAddress,
                new()
                {
                    ThingId = thingId,
                    User = await GetWalletAddressFor(accountName)
                }
            );

        return (int)index;
    }

    public async Task ClaimSettlementProposalAssessmentVerifierLotterySpotAs(
        string accountName, byte[] thingProposalId, ushort thingVerifiersArrayIndex
    )
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _settlementProposalAssessmentVerifierLotteryAddress,
            message: new ClaimSettlementProposalAssessmentVerifierLotterySpotMessage
            {
                ThingProposalId = thingProposalId,
                ThingVerifiersArrayIndex = thingVerifiersArrayIndex
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinSettlementProposalAssessmentVerifierLotteryAs(string accountName, byte[] thingProposalId, byte[] userData)
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _settlementProposalAssessmentVerifierLotteryAddress,
            message: new JoinSettlementProposalAssessmentVerifierLotteryMessage
            {
                ThingProposalId = thingProposalId,
                UserData = userData
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task CastSettlementProposalAssessmentPollVoteAs(
        string accountName, byte[] thingProposalId, ushort settlementProposalVerifiersArrayIndex, Vote vote
    )
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _settlementProposalAssessmentPollAddress,
            message: new CastSettlementProposalAssessmentPollVoteMessage
            {
                ThingProposalId = thingProposalId,
                SettlementProposalVerifiersArrayIndex = settlementProposalVerifiersArrayIndex,
                Vote = vote
            }
        );

        await _blockchainManipulator.Mine(1);
    }
}
