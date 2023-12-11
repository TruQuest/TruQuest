import 'dart:async';
import 'dart:convert';

import '../../../general/utils/logger.dart';
import '../../../user/services/user_service.dart';
import '../../errors/user_operation_error.dart';
import '../../services/ethereum_api_service.dart';
import '../../../ethereum_js_interop.dart';
import '../../../widget_extensions.dart';
import '../../../general/contracts/erc4337/ientrypoint_contract.dart';
import '../../../general/utils/utils.dart';
import '../../../general/contracts/erc4337/iaccount_factory_contract.dart';

class UserOperation {
  final String sender;
  final BigInt nonce;
  final String initCode;
  final String callData;
  final BigInt callGasLimit;
  final BigInt verificationGasLimit;
  final BigInt preVerificationGas;
  final BigInt maxFeePerGas;
  final BigInt maxPriorityFeePerGas;
  final String paymasterAndData;
  final String signature;

  BigInt get totalProvisionedGas => callGasLimit + verificationGasLimit + preVerificationGas;

  late final UserOperationBuilder builder;

  ErrorDescription? Function(String data)? parseError;

  UserOperation({
    required this.sender,
    required this.nonce,
    required this.initCode,
    required this.callData,
    required this.callGasLimit,
    required this.verificationGasLimit,
    required this.preVerificationGas,
    required this.maxFeePerGas,
    required this.maxPriorityFeePerGas,
    required this.paymasterAndData,
    required this.signature,
  });

  static UserOperationBuilder create() => resolveDependency<UserOperationBuilder>();

  static UserOperationBuilder createFrom(UserOperation userOp) => UserOperationBuilder._(
        userOp.builder._userService,
        userOp.builder._ethereumApiService,
        userOp.builder._entryPointContract,
        userOp.builder._accountFactoryContract,
        userOp.builder._sender,
        userOp.builder._initCode,
        userOp.builder._callData,
      );

  Map<String, dynamic> toJson() => {
        'sender': sender,
        'nonce': nonce.toHex(),
        'initCode': initCode,
        'callData': callData,
        'callGasLimit': callGasLimit.toHex(),
        'verificationGasLimit': verificationGasLimit.toHex(),
        'preVerificationGas': preVerificationGas.toHex(),
        'maxFeePerGas': maxFeePerGas.toHex(),
        'maxPriorityFeePerGas': maxPriorityFeePerGas.toHex(),
        'paymasterAndData': paymasterAndData,
        'signature': signature,
      };

  @override
  String toString() => const JsonEncoder.withIndent('  ').convert(toJson());

  List<dynamic> toList() => [
        sender,
        nonce,
        initCode,
        callData,
        callGasLimit,
        verificationGasLimit,
        preVerificationGas,
        maxFeePerGas,
        maxPriorityFeePerGas,
        paymasterAndData,
        signature,
      ];

  UserOperation copyWith({
    BigInt? callGasLimit,
    BigInt? verificationGasLimit,
    BigInt? preVerificationGas,
    BigInt? maxFeePerGas,
    BigInt? maxPriorityFeePerGas,
    String? signature,
  }) =>
      UserOperation(
        sender: sender,
        nonce: nonce,
        initCode: initCode,
        callData: callData,
        callGasLimit: callGasLimit ?? this.callGasLimit,
        verificationGasLimit: verificationGasLimit ?? this.verificationGasLimit,
        preVerificationGas: preVerificationGas ?? this.preVerificationGas,
        maxFeePerGas: maxFeePerGas ?? this.maxFeePerGas,
        maxPriorityFeePerGas: maxPriorityFeePerGas ?? this.maxPriorityFeePerGas,
        paymasterAndData: paymasterAndData,
        signature: signature ?? this.signature,
      );
}

class UserOperationBuilder {
  final UserService _userService;
  final EthereumApiService _ethereumApiService;
  final IEntryPointContract _entryPointContract;
  final IAccountFactoryContract _accountFactoryContract;

  late final String _sender;
  late final BigInt _nonce;
  late final String _initCode;
  late final String _callData;
  String _paymasterAndData = '0x';

  late UserOperation _userOp;

  final List<Future Function()> _tasks = [];

  late final BigInt estimatedGasCost;

  UserOperationBuilder(
    this._userService,
    this._ethereumApiService,
    this._entryPointContract,
    this._accountFactoryContract,
  ) {
    _sender = _userService.latestCurrentUser!.walletAddress!;
    _tasks.add(() async {
      var code = await _ethereumApiService.getCode(_sender);
      _initCode =
          code == '0x' ? _accountFactoryContract.getInitCode(_userService.latestCurrentUser!.signerAddress!) : '0x';
    });
  }

  UserOperationBuilder._(
    this._userService,
    this._ethereumApiService,
    this._entryPointContract,
    this._accountFactoryContract,
    this._sender,
    this._initCode,
    this._callData,
  );

  UserOperationBuilder _withCurrentNonce() {
    _tasks.add(() async {
      _nonce = await _entryPointContract.getNonce(_sender);
    });

    return this;
  }

  UserOperationBuilder action((String, String) targetAndCallData) {
    _callData = _accountFactoryContract.accountContract.execute(targetAndCallData);
    return this;
  }

  UserOperationBuilder actions(List<(String, String)> targetAndCallDataList) {
    _callData = _accountFactoryContract.accountContract.executeBatch(targetAndCallDataList);
    return this;
  }

  UserOperationBuilder _withEstimatedGasLimits() {
    _tasks.add(() async {
      _userOp = UserOperation(
        sender: _sender,
        nonce: _nonce,
        initCode: _initCode,
        callData: _callData,
        callGasLimit: BigInt.zero,
        verificationGasLimit: BigInt.zero,
        preVerificationGas: BigInt.zero,
        maxFeePerGas: BigInt.zero,
        maxPriorityFeePerGas: BigInt.zero,
        paymasterAndData: _paymasterAndData,
        signature: _accountFactoryContract.dummySignatureForGasEstimation,
      );

      var fees = await _ethereumApiService.estimateUserOperationGas(_userOp);
      var (preVerificationGas, verificationGasLimit, callGasLimit) = fees;

      logger.info('PreVerificationGas: $preVerificationGas');
      logger.info('VerificationGasLimit: $verificationGasLimit');
      logger.info('CallGasLimit: $callGasLimit');

      _userOp = _userOp.copyWith(
        callGasLimit: callGasLimit,
        verificationGasLimit: verificationGasLimit,
        preVerificationGas: preVerificationGas,
      );
    });

    return this;
  }

  UserOperationBuilder _withCurrentGasPrice() {
    _tasks.add(() async {
      /*
        Source: https://docs.alchemy.com/reference/bundler-api-fee-logic

        Recommended Actions for Calculating maxFeePerGas:
            - Fetch Current Base Fee: Use the method eth_getBlockByNumber with the 'latest' parameter to get the current baseFeePerGas.
            - Apply Buffer on Base Fee: To account for potential fee changes, apply a buffer on the current base fee
            based on the requirements in the table shown above (5% for Arbitrum Mainnet and 50% for all other mainnets)
            - Fetch Current Priority Fee with Rundler: Use the rundler_maxPriorityFeePerGas method to query the current priority fee for the network.
            - Apply Buffer on Priority Fee: Once you have the current priority fee using rundler_maxPriorityFeePerGas,
            increase it according to the fee requirement table shown above for any unexpected changes (No buffer for Arbitrum Mainnet and
            25% buffer for all other mainnets).
            - Determine maxFeePerGas: Add the buffered values from steps 2 and 4 together to obtain the maxFeePerGas for your user operation.
      */

      var baseFee = await _ethereumApiService.getBaseFee();
      if (baseFee == null) throw UserOperationError(message: 'Error trying to get current base fee');

      logger.info('Base fee: 0x${baseFee.toRadixString(16)} WEI');
      baseFee = BigInt.from((baseFee * BigInt.from(3)) / BigInt.two);
      logger.info('Base fee bid (+ 50% buffer): 0x${baseFee.toRadixString(16)} WEI');

      var maxPriorityFee = await _ethereumApiService.getMaxPriorityFee();
      if (maxPriorityFee == null) throw UserOperationError(message: 'Error trying to get current max priority fee');

      logger.info('Max priority fee: 0x${maxPriorityFee.toRadixString(16)} WEI');
      maxPriorityFee = BigInt.from((maxPriorityFee * BigInt.from(5)) / BigInt.from(4));
      logger.info('Max priority fee bid (+ 25% buffer): 0x${maxPriorityFee.toRadixString(16)} WEI');

      var maxFeeBid = baseFee + maxPriorityFee;
      logger.info('Max fee bid: 0x${maxFeeBid.toRadixString(16)} WEI');

      _userOp = _userOp.copyWith(
        maxFeePerGas: maxFeeBid,
        maxPriorityFeePerGas: maxPriorityFee,
      );

      estimatedGasCost = _userOp.totalProvisionedGas * _userOp.maxFeePerGas;

      logger.info('Estimated gas cost: ${formatUnits(BigNumber.from(estimatedGasCost.toString()))} ETH');
    });

    return this;
  }

  Future<UserOperation> unsigned() async {
    _withCurrentNonce()._withEstimatedGasLimits()._withCurrentGasPrice();

    for (var task in _tasks) await task();

    _userOp.builder = this;

    return _userOp;
  }

  Future<UserOperation> signed() async {
    _withCurrentNonce()._withEstimatedGasLimits()._withCurrentGasPrice();

    _tasks.add(() async {
      var userOpHash = await _entryPointContract.getUserOpHash(_userOp);
      _userOp = _userOp.copyWith(
        signature: await _userService.personalSignDigest(userOpHash),
      );
    });

    for (var task in _tasks) await task();

    return _userOp;
  }
}
