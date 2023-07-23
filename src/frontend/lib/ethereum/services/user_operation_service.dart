import 'ethereum_api_service.dart';
import '../errors/user_operation_error.dart';
import '../models/im/user_operation.dart';
import '../models/vm/get_user_operation_receipt_rvm.dart';

class UserOperationService {
  final EthereumApiService _ethereumApiService;

  UserOperationService(this._ethereumApiService);

  Future sendBatch({
    required List<(String, String)> actions,
    int confirmations = 1,
  }) async {
    if (actions.isEmpty) throw UserOperationError('No actions specified');

    int attempts = 5;
    UserOperation userOp;
    UserOperationError? error;
    String? userOpHash;
    double preVerificationGasMultiplier = 1.1,
        verificationGasLimitMultiplier = 1.5,
        callGasLimitMultiplier = actions.length * 3.0;

    do {
      error = null;

      var userOpBuilder = UserOperation.create()
          .withEstimatedGasLimitsMultipliers(
              preVerificationGasMultiplier: preVerificationGasMultiplier,
              verificationGasLimitMultiplier: verificationGasLimitMultiplier,
              callGasLimitMultiplier: callGasLimitMultiplier);

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
      userOpHash != null && error == null ||
          userOpHash == null && error != null,
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
