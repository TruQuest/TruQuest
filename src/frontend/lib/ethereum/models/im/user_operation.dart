import 'dart:async';
import 'dart:convert';

import '../vm/smart_wallet.dart';
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

  static UserOperationBuilder create() =>
      resolveDependency<UserOperationBuilder>();

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
  String toString() => jsonEncode(toJson());

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
  final EthereumApiService _ethereumApiService;
  final IEntryPointContract _entryPointContract;
  final IAccountFactoryContract _accountFactoryContract;

  late final SmartWallet _wallet;
  late final String _sender;
  late final BigInt _nonce;
  late final String _initCode;
  late final String _callData;
  String _paymasterAndData = '0x';

  late UserOperation _userOp;

  final List<Future Function()> _tasks = [];

  UserOperationBuilder(
    this._ethereumApiService,
    this._entryPointContract,
    this._accountFactoryContract,
  );

  UserOperationBuilder from(SmartWallet wallet) {
    _wallet = wallet;
    _sender = wallet.currentWalletAddress;
    _tasks.add(() async {
      var code = await _ethereumApiService.getCode(_sender);
      _initCode = code == '0x'
          ? _accountFactoryContract.getInitCode(_wallet.currentOwnerAddress)
          : '0x';
    });

    return this;
  }

  UserOperationBuilder _withCurrentNonce() {
    _tasks.add(() async {
      _nonce = await _entryPointContract.getNonce(_sender);
    });

    return this;
  }

  UserOperationBuilder action((String, String) targetAndCallData) {
    _callData = _accountFactoryContract.accountContract.execute(
      targetAndCallData,
    );
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

      _userOp = _userOp.copyWith(
        callGasLimit: fees!['callGasLimit']! * BigInt.two,
        verificationGasLimit: fees['verificationGasLimit']! * BigInt.two,
        preVerificationGas: fees['preVerificationGas']! * BigInt.two,
      );
    });

    return this;
  }

  UserOperationBuilder _withCurrentGasPrice() {
    _tasks.add(() async {
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

      maxPriorityFeeBid = BigInt.zero;
      print(
        'Max priority fee bid: 0x${maxPriorityFeeBid.toRadixString(16)} WEI',
      );

      var maxFeeBid = baseFee * BigInt.two + maxPriorityFeeBid;
      print('Max fee bid: 0x${maxFeeBid.toRadixString(16)} WEI');

      _userOp = _userOp.copyWith(
        maxFeePerGas: maxFeeBid,
        maxPriorityFeePerGas: maxPriorityFeeBid,
      );

      var estimatedGasCost = (_userOp.preVerificationGas +
              _userOp.verificationGasLimit +
              _userOp.callGasLimit) *
          _userOp.maxFeePerGas;

      print('Estimated gas cost: $estimatedGasCost WEI');
      print(
        'Estimated gas cost: ${formatUnits(BigNumber.from(estimatedGasCost.toString()))} ETH',
      );
    });

    return this;
  }

  Future<UserOperation> signed() async {
    _withCurrentNonce()._withEstimatedGasLimits()._withCurrentGasPrice();

    _tasks.add(() async {
      var userOpHash = await _entryPointContract.getUserOpHash(_userOp);
      _userOp = _userOp.copyWith(
        signature: _wallet.ownerSignDigest(userOpHash),
      );
    });

    for (var task in _tasks) {
      await task();
    }

    return _userOp;
  }
}
