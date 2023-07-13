import '../../../general/utils/utils.dart';

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
    this.paymasterAndData = '0x',
    this.signature = '0x',
  });

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
