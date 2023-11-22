import 'dart:async';
import 'dart:convert';

import '../../../user/services/user_service.dart';
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

  double _preVerificationGasMultiplier = 1;
  double get preVerificationGasMultiplier => _preVerificationGasMultiplier;
  double _verificationGasLimitMultiplier = 1;
  double get verificationGasLimitMultiplier => _verificationGasLimitMultiplier;
  double _callGasLimitMultiplier = 1;
  double get callGasLimitMultiplier => _callGasLimitMultiplier;

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

  UserOperationBuilder withEstimatedGasLimitsMultipliers({
    double preVerificationGasMultiplier = 1,
    double verificationGasLimitMultiplier = 1,
    double callGasLimitMultiplier = 1,
  }) {
    _preVerificationGasMultiplier = preVerificationGasMultiplier;
    _verificationGasLimitMultiplier = verificationGasLimitMultiplier;
    _callGasLimitMultiplier = callGasLimitMultiplier;

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

      print('PreVerificationGas: $preVerificationGas');
      preVerificationGas = BigInt.from(
        preVerificationGas.toDouble() * _preVerificationGasMultiplier,
      );
      print('PreVerificationGas [Provisioned]: $preVerificationGas');

      print('VerificationGasLimit: $verificationGasLimit');
      verificationGasLimit = BigInt.from(
        verificationGasLimit.toDouble() * _verificationGasLimitMultiplier,
      );
      print('VerificationGasLimit [Provisioned]: $verificationGasLimit');

      print('CallGasLimit: $callGasLimit');
      callGasLimit = BigInt.from(
        callGasLimit.toDouble() * _callGasLimitMultiplier,
      );
      print('CallGasLimit [Provisioned]: $callGasLimit');

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
      // @@TODO!!: Use Alchemy's API for fees data.
      var minPriorityFeeBid = BigInt.parse('1000000000'); // 1 GWEI

      var baseFee = await _ethereumApiService.getBaseFee();
      print('Base fee: 0x${baseFee!.toRadixString(16)} WEI');

      var maxPriorityFee = await _ethereumApiService.getMaxPriorityFee();
      print('Max priority fee: 0x${maxPriorityFee!.toRadixString(16)} WEI');

      var maxPriorityFeeBid = BigInt.from(
        (maxPriorityFee * BigInt.from(4)) / BigInt.from(3),
      );
      if (maxPriorityFeeBid < minPriorityFeeBid) {
        maxPriorityFeeBid = minPriorityFeeBid;
      }

      print('Max priority fee bid: 0x${maxPriorityFeeBid.toRadixString(16)} WEI');

      var maxFeeBid = baseFee * BigInt.two + maxPriorityFeeBid;
      print('Max fee bid: 0x${maxFeeBid.toRadixString(16)} WEI');

      _userOp = _userOp.copyWith(
        maxFeePerGas: maxFeeBid,
        maxPriorityFeePerGas: maxPriorityFeeBid,
      );

      estimatedGasCost = _userOp.totalProvisionedGas * _userOp.maxFeePerGas;

      print('Estimated gas cost: ${formatUnits(BigNumber.from(estimatedGasCost.toString()))} ETH');
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
