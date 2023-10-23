import 'dart:async';

import '../errors/wallet_action_declined_error.dart';
import '../models/vm/user_operation_vm.dart';
import 'ethereum_api_service.dart';
import '../errors/user_operation_error.dart';
import '../models/im/user_operation.dart';
import '../models/vm/get_user_operation_receipt_rvm.dart';

class UserOperationService {
  final EthereumApiService _ethereumApiService;

  UserOperationService(this._ethereumApiService);

  Stream<UserOperationVm> prepareOneWithRealTimeFeeUpdates({
    required List<(String, String)> actions,
    String functionSignature = '',
    String description = '',
    BigInt? stakeSize,
  }) {
    var canceled = Completer();
    var channel = StreamController<UserOperationVm>(onCancel: () => canceled.complete());
    channel.onListen = () => _keepRefreshingUserOpUntilCanceled(
          actions,
          description,
          functionSignature,
          stakeSize,
          channel.sink,
          canceled,
        );

    return channel.stream;
  }

  void _keepRefreshingUserOpUntilCanceled(
    List<(String, String)> actions,
    String description,
    String functionSignature,
    BigInt? stakeSize,
    Sink<UserOperationVm> sink,
    Completer canceled,
  ) async {
    while (!canceled.isCompleted) {
      var userOp = await _createUnsignedFromBatch(actions: actions);
      if (userOp == null) {
        sink.close();
        return;
      }

      if (canceled.isCompleted) return;

      sink.add(
        UserOperationVm(
          userOp,
          userOp.sender,
          functionSignature,
          description,
          stakeSize,
          userOp.totalProvisionedGas,
          userOp.builder.estimatedGasCost,
          false,
        ),
      );

      await Future.delayed(const Duration(seconds: 10)); // @@TODO: Config.
    }
  }

  Future<UserOperation?> _createUnsignedFromBatch({required List<(String, String)> actions}) async {
    assert(actions.isNotEmpty);

    double preVerificationGasMultiplier = 1, verificationGasLimitMultiplier = 1, callGasLimitMultiplier = 1;

    var userOpBuilder = UserOperation.create().withEstimatedGasLimitsMultipliers(
      preVerificationGasMultiplier: preVerificationGasMultiplier,
      verificationGasLimitMultiplier: verificationGasLimitMultiplier,
      callGasLimitMultiplier: callGasLimitMultiplier,
    );

    if (actions.length == 1) {
      userOpBuilder = userOpBuilder.action((actions.first.$1, actions.first.$2));
    } else {
      userOpBuilder = userOpBuilder.actions(actions);
    }

    try {
      return await userOpBuilder.unsigned();
    } on UserOperationError catch (error) {
      print(error);
      return null;
    }
  }

  Future<UserOperationError?> send(UserOperation userOp, {int confirmations = 1}) async {
    int attempts = 5; // @@TODO: Config.
    UserOperationError? error;
    String? userOpHash;
    double preVerificationGasMultiplier = userOp.builder.preVerificationGasMultiplier;
    double verificationGasLimitMultiplier = userOp.builder.verificationGasLimitMultiplier;
    double callGasLimitMultiplier = userOp.builder.callGasLimitMultiplier;

    do {
      error = null;

      try {
        userOp = await UserOperation.createFrom(userOp)
            .withEstimatedGasLimitsMultipliers(
              preVerificationGasMultiplier: preVerificationGasMultiplier,
              verificationGasLimitMultiplier: verificationGasLimitMultiplier,
              callGasLimitMultiplier: callGasLimitMultiplier,
            )
            .signed();

        print('UserOp[$attempts]:\n$userOp');

        userOpHash = await _ethereumApiService.sendUserOperation(userOp);
      } on WalletActionDeclinedError catch (e) {
        print(e);
        error = UserOperationError(e.message);
        break;
      } on UserOperationError catch (e) {
        print(e);
        error = e;
        if (e.isPreVerificationGasTooLow) {
          preVerificationGasMultiplier += 0.05; // @@TODO: Config.
        } else if (e.isOverVerificationGasLimit) {
          verificationGasLimitMultiplier += 0.2; // @@TODO: Config.
        } else if (!e.isRetryable) {
          // e.g. e.isPastOrFutureExecutionRevertError
          break;
        }
      }
    } while (userOpHash == null && --attempts > 0);

    assert(userOpHash != null && error == null || userOpHash == null && error != null);

    if (error != null) return error;

    print('UserOp Hash: $userOpHash');

    GetUserOperationReceiptRvm? receipt;
    do {
      await Future.delayed(const Duration(seconds: 2)); // @@TODO: Config.
      receipt = await _ethereumApiService.getUserOperationReceipt(userOpHash!);
      // @@??: Break waiting if !success ?
    } while (receipt == null || receipt.receipt.confirmations < confirmations);

    print('Receipt:\n$receipt');

    if (!receipt.success) {
      print('UserOp Execution Failed. Reason: ${receipt.reason ?? 'Unspecified'}');
      return UserOperationError(receipt.reason);
    }

    return null;
  }
}
