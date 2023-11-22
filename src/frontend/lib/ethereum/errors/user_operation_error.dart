import '../../general/errors/error.dart';

class UserOperationError extends Error {
  final int? code;
  final bool extractedFromEvent;

  bool get isPreVerificationGasTooLow => code == -32602 && message.startsWith('preVerificationGas too low');

  bool get isOverVerificationGasLimit =>
      code == -32500 && message == 'account validation failed: AA40 over verificationGasLimit';

  bool get isPastOrFutureExecutionRevertError => code == -32521;

  bool get isRetryable => !isPastOrFutureExecutionRevertError;

  const UserOperationError({this.code, String message = 'Something went wrong'})
      : extractedFromEvent = false,
        super(message);

  const UserOperationError.customContractError(String message)
      : code = -32521,
        extractedFromEvent = true,
        super(message);

  UserOperationError.fromMap(Map<String, dynamic> map)
      : code = map['code'],
        extractedFromEvent = false,
        super(map['message']);
}
