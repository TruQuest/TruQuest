import '../../general/errors/error.dart';

class UserOperationError extends Error {
  final int? code;

  bool get isPreVerificationGasTooLow =>
      code == -32602 && message.startsWith('preVerificationGas too low');
  bool get isOverVerificationGasLimit =>
      code == -32500 &&
      message == 'account validation failed: AA40 over verificationGasLimit';

  UserOperationError(String? reason)
      : code = null,
        super(reason ?? 'Unspecified reason');

  UserOperationError.fromMap(Map<String, dynamic> map)
      : code = map['code'],
        super(map['message']);
}
