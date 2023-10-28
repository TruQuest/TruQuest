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
    private readonly AccountProvider _accountProvider;
    private readonly UserOperationService _userOperationService;
    private readonly BlockchainManipulator _blockchainManipulator;

    private readonly Web3 _web3;
    private readonly string _rpcUrl;
    private readonly string _simpleAccountFactoryAddress;
    private readonly string _truthserumAddress;
    private readonly string _truQuestAddress;
    private readonly string _thingSubmissionVerifierLotteryAddress;
    private readonly string _acceptancePollAddress;
    private readonly string _thingAssessmentVerifierLotteryAddress;
    private readonly string _assessmentPollAddress;

    public ContractCaller(
        ILogger logger,
        IConfiguration configuration,
        AccountProvider accountProvider,
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
        _thingSubmissionVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingSubmissionVerifierLottery:Address"]!;
        _acceptancePollAddress = configuration[$"Ethereum:Contracts:{network}:AcceptancePoll:Address"]!;
        _thingAssessmentVerifierLotteryAddress = configuration[$"Ethereum:Contracts:{network}:ThingAssessmentVerifierLottery:Address"]!;
        _assessmentPollAddress = configuration[$"Ethereum:Contracts:{network}:AssessmentPoll:Address"]!;
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

    public async Task JoinThingSubmissionVerifierLotteryAs(string accountName, byte[] thingId, byte[] userData)
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _thingSubmissionVerifierLotteryAddress,
            message: new JoinThingSubmissionVerifierLotteryMessage
            {
                ThingId = thingId,
                UserData = userData
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task CastAcceptancePollVoteAs(
        string accountName, byte[] thingId, ushort thingVerifiersArrayIndex, Vote vote
    )
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _acceptancePollAddress,
            message: new CastAcceptancePollVoteMessage
            {
                ThingId = thingId,
                ThingVerifiersArrayIndex = thingVerifiersArrayIndex,
                Vote = vote
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task FundThingSettlementProposalAs(
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
            message: new FundThingSettlementProposalMessage
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
                _acceptancePollAddress,
                new()
                {
                    ThingId = thingId,
                    User = await GetWalletAddressFor(accountName)
                }
            );

        return (int)index;
    }

    public async Task ClaimThingAssessmentVerifierLotterySpotAs(
        string accountName, byte[] thingProposalId, ushort thingVerifiersArrayIndex
    )
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _thingAssessmentVerifierLotteryAddress,
            message: new ClaimLotterySpotMessage
            {
                ThingProposalId = thingProposalId,
                ThingVerifiersArrayIndex = thingVerifiersArrayIndex
            }
        );
        // catch (SmartContractCustomErrorRevertException ex)
        // {
        //     if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__AlreadyCommittedToLotteryError>())
        //     {
        //         var error = ex.DecodeError<ThingAssessmentVerifierLottery__AlreadyCommittedToLotteryError>();
        //     }
        //     else if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__LotteryExpiredError>())
        //     {
        //         var error = ex.DecodeError<ThingAssessmentVerifierLottery__LotteryExpiredError>();
        //     }
        //     else if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__LotteryNotActiveError>())
        //     {
        //         var error = ex.DecodeError<ThingAssessmentVerifierLottery__LotteryNotActiveError>();
        //     }
        //     else if (ex.IsCustomErrorFor<ThingAssessmentVerifierLottery__NotEnoughFundsError>())
        //     {
        //         var error = ex.DecodeError<ThingAssessmentVerifierLottery__NotEnoughFundsError>();
        //     }
        // }

        await _blockchainManipulator.Mine(1);
    }

    public async Task JoinThingAssessmentVerifierLotteryAs(string accountName, byte[] thingProposalId, byte[] userData)
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _thingAssessmentVerifierLotteryAddress,
            message: new JoinThingAssessmentVerifierLotteryMessage
            {
                ThingProposalId = thingProposalId,
                UserData = userData
            }
        );

        await _blockchainManipulator.Mine(1);
    }

    public async Task CastAssessmentPollVoteAs(
        string accountName, byte[] thingProposalId, ushort proposalVerifiersArrayIndex, Vote vote
    )
    {
        await _userOperationService.Send(
            signer: _accountProvider.GetAccount(accountName),
            targetAddress: _assessmentPollAddress,
            message: new CastAssessmentPollVoteMessage
            {
                ThingProposalId = thingProposalId,
                ProposalVerifiersArrayIndex = proposalVerifiersArrayIndex,
                Vote = vote
            }
        );

        await _blockchainManipulator.Mine(1);
    }
}
