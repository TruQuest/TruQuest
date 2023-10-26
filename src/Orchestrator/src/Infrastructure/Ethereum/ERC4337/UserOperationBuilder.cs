using System.Numerics;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3.Accounts;

using Application.Common.Interfaces;
using Application.Ethereum.Common.Models.IM;

using Infrastructure.Ethereum.Messages;

namespace Infrastructure.Ethereum.ERC4337;

internal class UserOperationBuilder
{
    private readonly ILogger<UserOperationBuilder> _logger;
    private readonly IContractCaller _contractCaller;
    private readonly IL2BlockchainQueryable _l2BlockchainQueryable;
    private readonly BundlerApi _bundlerApi;
    private readonly AbiEncoder _abiEncoder;
    private readonly EthereumMessageSigner _personalSigner;

    private readonly string _simpleAccountFactoryAddress;

    private Account _owner;
    private string _sender;
    private BigInteger _nonce;
    private string _initCode;
    private string _callData;
    private string _paymasterAndData = "0x";

    private UserOperation _userOp;

    private float _preVerificationGasMultiplier = 1;
    private float _verificationGasLimitMultiplier = 1;
    private float _callGasLimitMultiplier = 1;

    private readonly List<Func<Task>> _tasks = new();

    public UserOperationBuilder(
        ILogger<UserOperationBuilder> logger,
        IConfiguration configuration,
        IContractCaller contractCaller,
        IL2BlockchainQueryable l2BlockchainQueryable,
        BundlerApi bundlerApi,
        AbiEncoder abiEncoder,
        EthereumMessageSigner personalSigner
    )
    {
        _logger = logger;
        _contractCaller = contractCaller;
        _l2BlockchainQueryable = l2BlockchainQueryable;
        _bundlerApi = bundlerApi;
        _abiEncoder = abiEncoder;
        _personalSigner = personalSigner;

        var network = configuration["Ethereum:Network"]!;
        _simpleAccountFactoryAddress = configuration[$"Ethereum:Contracts:{network}:SimpleAccountFactory:Address"]!;
    }

    public UserOperationBuilder From(Account account)
    {
        _owner = account;
        _tasks.Add(async () =>
        {
            _sender = await _contractCaller.GetWalletAddressFor(_owner.Address);
            bool deployed = await _l2BlockchainQueryable.CheckContractDeployed(_sender);
            if (!deployed)
            {
                var data = _abiEncoder.EncodeFunctionData(new CreateAccountMessage
                {
                    Owner = _owner.Address,
                    Salt = 0
                });
                _initCode = _simpleAccountFactoryAddress + data.Substring(2);
            }
            else
            {
                _initCode = "0x";
            }
        });

        return this;
    }

    private UserOperationBuilder _withCurrentNonce()
    {
        _tasks.Add(async () =>
        {
            _nonce = await _contractCaller.GetWalletNonce(_sender);
        });
        return this;
    }

    public UserOperationBuilder Action(string targetAddress, FunctionMessage message)
    {
        _callData = _abiEncoder.EncodeFunctionData(new ExecuteMessage
        {
            Dest = targetAddress,
            Value = 0,
            Func = _abiEncoder.EncodeFunctionData(message).HexToByteArray()
        });
        return this;
    }

    public UserOperationBuilder Actions(List<(string TargetAddress, FunctionMessage Message)> targetAndMessageList)
    {
        _callData = _abiEncoder.EncodeFunctionData(new ExecuteBatchMessage
        {
            Dest = targetAndMessageList.Select(e => e.TargetAddress).ToList(),
            Value = targetAndMessageList.Select(_ => BigInteger.Zero).ToList(),
            Func = targetAndMessageList
                .Select(e => _abiEncoder.EncodeFunctionData(e.Message).HexToByteArray())
                .ToList()
        });
        return this;
    }

    public UserOperationBuilder WithEstimatedGasLimitsMultipliers(
        float preVerificationGasMultiplier = 1,
        float verificationGasLimitMultiplier = 1,
        float callGasLimitMultiplier = 1
    )
    {
        _preVerificationGasMultiplier = preVerificationGasMultiplier;
        _verificationGasLimitMultiplier = verificationGasLimitMultiplier;
        _callGasLimitMultiplier = callGasLimitMultiplier;

        return this;
    }

    private UserOperationBuilder _withEstimatedGasLimits()
    {
        _tasks.Add(async () =>
        {
            _userOp = new UserOperation
            {
                Sender = _sender,
                Nonce = _nonce,
                InitCode = _initCode,
                CallData = _callData,
                CallGasLimit = 0,
                VerificationGasLimit = 0,
                PreVerificationGas = 0,
                MaxFeePerGas = 0,
                MaxPriorityFeePerGas = 0,
                PaymasterAndData = _paymasterAndData,
                Signature = "0xfffffffffffffffffffffffffffffff0000000000000000000000000000000007aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa1c",
            };

            var gasEstimations = await _bundlerApi.EstimateUserOperationGas(_userOp);
            var (preVerificationGas, verificationGasLimit, callGasLimit) = gasEstimations.Value;

            _logger.LogInformation($"PreVerificationGas: {preVerificationGas}");
            preVerificationGas = new BigInteger((float)preVerificationGas * _preVerificationGasMultiplier);
            _logger.LogInformation($"PreVerificationGas [Provisioned]: {preVerificationGas}");

            _logger.LogInformation($"VerificationGasLimit: {verificationGasLimit}");
            verificationGasLimit = new BigInteger((float)verificationGasLimit * _verificationGasLimitMultiplier);
            _logger.LogInformation($"VerificationGasLimit [Provisioned]: {verificationGasLimit}");

            _logger.LogInformation($"CallGasLimit: {callGasLimit}");
            callGasLimit = new BigInteger((float)callGasLimit * _callGasLimitMultiplier);
            _logger.LogInformation($"CallGasLimit [Provisioned]: {callGasLimit}");

            _userOp = _userOp with
            {
                PreVerificationGas = preVerificationGas,
                VerificationGasLimit = verificationGasLimit,
                CallGasLimit = callGasLimit
            };
        });

        return this;
    }

    private UserOperationBuilder _withCurrentGasPrice()
    {
        _tasks.Add(async () =>
        {
            var minPriorityFeeBid = BigInteger.Parse("1000000000"); // 1 GWEI

            var baseFee = await _l2BlockchainQueryable.GetBaseFee();
            _logger.LogInformation($"Base fee: {new HexBigInteger(baseFee).HexValue} WEI");

            var maxPriorityFee = await _l2BlockchainQueryable.GetMaxPriorityFee();
            _logger.LogInformation($"Max priority fee: {new HexBigInteger(maxPriorityFee).HexValue} WEI");

            var maxPriorityFeeBid = new BigInteger((float)maxPriorityFee * 1.33f);
            if (maxPriorityFeeBid < minPriorityFeeBid)
            {
                maxPriorityFeeBid = minPriorityFeeBid;
            }

            _logger.LogInformation($"Max priority fee bid: {new HexBigInteger(maxPriorityFeeBid).HexValue} WEI");

            var maxFeeBid = baseFee * 2 + maxPriorityFeeBid;
            _logger.LogInformation($"Max fee bid: {new HexBigInteger(maxFeeBid).HexValue} WEI");

            _userOp = _userOp with
            {
                MaxFeePerGas = maxFeeBid,
                MaxPriorityFeePerGas = maxPriorityFeeBid
            };

            var estimatedGasCost = (_userOp.PreVerificationGas + _userOp.VerificationGasLimit + _userOp.CallGasLimit) *
                _userOp.MaxFeePerGas;

            _logger.LogInformation($"Estimated gas cost: {UnitConversion.Convert.FromWei(estimatedGasCost)} ETH");
        });

        return this;
    }

    public async Task<UserOperation> Signed()
    {
        _withCurrentNonce()._withEstimatedGasLimits()._withCurrentGasPrice();

        _tasks.Add(async () =>
        {
            var userOpHash = await _contractCaller.GetUserOperationHash(_userOp);
            _userOp = _userOp with
            {
                Signature = _personalSigner.Sign(userOpHash, new EthECKey(_owner.PrivateKey))
            };
        });

        foreach (var task in _tasks)
        {
            await task();
        }

        return _userOp;
    }
}
