import 'dart:async';

import '../errors/wallet_action_declined_error.dart';
import 'ethereum_api_service.dart';
import '../errors/user_operation_error.dart';
import '../models/im/user_operation.dart';
import '../models/vm/get_user_operation_receipt_rvm.dart';

class UserOperationService {
  final EthereumApiService _ethereumApiService;

  UserOperationService(this._ethereumApiService);

  Stream<UserOperation> prepareOneWithRealTimeFeeUpdates({
    required List<(String, String)> actions,
  }) {
    var canceled = Completer();
    var channel = StreamController<UserOperation>(
      onCancel: () => canceled.complete(),
    );
    channel.onListen = () => _keepRefreshingUserOpUntilCanceled(actions, channel.sink, canceled);

    return channel.stream;
  }

  void _keepRefreshingUserOpUntilCanceled(
    List<(String, String)> actions,
    Sink<UserOperation> sink,
    Completer canceled,
  ) async {
    while (!canceled.isCompleted) {
      var userOp = await createUnsignedFromBatch(
        actions: actions,
      );

      if (canceled.isCompleted) {
        break;
      }

      sink.add(userOp);

      await Future.delayed(const Duration(seconds: 10)); // @@TODO: Config.
    }
  }

  Future<UserOperation> createUnsignedFromBatch({
    required List<(String, String)> actions,
  }) async {
    assert(actions.isNotEmpty);

    double preVerificationGasMultiplier = 1.1,
        verificationGasLimitMultiplier = 1.5,
        callGasLimitMultiplier = actions.length * 3.0;

    var userOpBuilder = UserOperation.create().withEstimatedGasLimitsMultipliers(
      preVerificationGasMultiplier: preVerificationGasMultiplier,
      verificationGasLimitMultiplier: verificationGasLimitMultiplier,
      callGasLimitMultiplier: callGasLimitMultiplier,
    );

    if (actions.length == 1) {
      userOpBuilder = userOpBuilder.action(
        (actions.first.$1, actions.first.$2),
      );
    } else {
      userOpBuilder = userOpBuilder.actions(actions);
    }

    return await userOpBuilder.unsigned();
  }

  Future<UserOperation> createUnsigned({
    required String target,
    required String action,
  }) =>
      createUnsignedFromBatch(actions: [(target, action)]);

  Future<UserOperationError?> sendUserOp(
    UserOperation userOp, {
    int confirmations = 1,
  }) async {
    int attempts = 5;
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
      } on WalletActionDeclinedError catch (error) {
        print(error.message);
        return UserOperationError(error.message);
      }

      print('UserOp[$attempts]:\n$userOp');

      var result = await _ethereumApiService.sendUserOperation(userOp);
      if (result.isLeft) {
        error = result.left;
        print('Error: [${error.code}]: ${error.message}');
        if (error.isPreVerificationGasTooLow) {
          preVerificationGasMultiplier += 0.05;
        } else if (error.isOverVerificationGasLimit) {
          verificationGasLimitMultiplier += 0.2;
        } else {
          break;
        }
      } else {
        userOpHash = result.right;
      }
    } while (userOpHash == null && --attempts > 0);

    assert(
      userOpHash != null && error == null || userOpHash == null && error != null,
    );

    if (error != null) {
      return error;
    }

    print('UserOp Hash: $userOpHash');

    GetUserOperationReceiptRvm? receipt;
    do {
      await Future.delayed(const Duration(seconds: 2));

      receipt = await _ethereumApiService.getUserOperationReceipt(
        userOpHash!,
      );
      // @@??: Break waiting if !success ?
    } while (receipt == null || receipt.receipt.confirmations < confirmations);

    print('Receipt:\n$receipt');

    if (!receipt.success) {
      print(
        'UserOp Execution Failed. Reason: ${receipt.reason ?? 'Unspecified'}',
      );
      return UserOperationError(receipt.reason);
    }

    return null;
  }

  Future sendBatch({
    required List<(String, String)> actions,
    int confirmations = 1,
  }) async {
    if (actions.isEmpty) throw const UserOperationError('No actions specified');

    int attempts = 5;
    UserOperation userOp;
    UserOperationError? error;
    String? userOpHash;
    double preVerificationGasMultiplier = 1.1,
        verificationGasLimitMultiplier = 1.5,
        callGasLimitMultiplier = actions.length * 3.0;

    do {
      error = null;

      var userOpBuilder = UserOperation.create().withEstimatedGasLimitsMultipliers(
        preVerificationGasMultiplier: preVerificationGasMultiplier,
        verificationGasLimitMultiplier: verificationGasLimitMultiplier,
        callGasLimitMultiplier: callGasLimitMultiplier,
      );

      if (actions.length == 1) {
        userOpBuilder = userOpBuilder.action(
          (actions.first.$1, actions.first.$2),
        );
      } else {
        userOpBuilder = userOpBuilder.actions(actions);
      }

      userOp = await userOpBuilder.signed();

      print('UserOp[$attempts]:\n$userOp');

      var result = await _ethereumApiService.sendUserOperation(userOp);
      if (result.isLeft) {
        error = result.left;
        print('Error: [${error.code}]: ${error.message}');
        if (error.isPreVerificationGasTooLow) {
          preVerificationGasMultiplier += 0.05;
        } else if (error.isOverVerificationGasLimit) {
          verificationGasLimitMultiplier += 0.2;
        } else {
          break;
        }
      } else {
        userOpHash = result.right;
      }
    } while (userOpHash == null && --attempts > 0);

    assert(
      userOpHash != null && error == null || userOpHash == null && error != null,
    );

    if (error != null) {
      throw error;
    }

    print('UserOp Hash: $userOpHash');

    GetUserOperationReceiptRvm? receipt;
    do {
      await Future.delayed(const Duration(seconds: 2));

      receipt = await _ethereumApiService.getUserOperationReceipt(
        userOpHash!,
      );
      // @@??: Break waiting if !success ?
    } while (receipt == null || receipt.receipt.confirmations < confirmations);

    print('Receipt:\n$receipt');

    if (!receipt.success) {
      print(
        'UserOp Execution Failed. Reason: ${receipt.reason ?? 'Unspecified'}',
      );
      throw UserOperationError(receipt.reason);
    }
  }

  Future send({
    required String target,
    required String action,
    int confirmations = 1,
  }) =>
      sendBatch(
        actions: [(target, action)],
        confirmations: confirmations,
      );
}
