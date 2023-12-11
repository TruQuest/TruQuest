import 'dart:async';

import 'package:either_dart/either.dart';

import '../../ethereum_js_interop.dart';
import '../../general/contracts/base_contract.dart';
import '../../general/contracts/erc4337/ientrypoint_contract.dart';
import '../../general/utils/logger.dart';
import '../../user/errors/get_credential_error.dart';
import '../errors/wallet_action_declined_error.dart';
import '../models/vm/user_operation_vm.dart';
import 'ethereum_api_service.dart';
import '../errors/user_operation_error.dart';
import '../models/im/user_operation.dart';
import '../models/vm/get_user_operation_receipt_rvm.dart';
import 'ethereum_rpc_provider.dart';

class UserOperationService {
  final EthereumRpcProvider _ethereumRpcProvider;
  final EthereumApiService _ethereumApiService;
  final IEntryPointContract _entryPointContract;

  UserOperationService(
    this._ethereumRpcProvider,
    this._ethereumApiService,
    this._entryPointContract,
  );

  Stream<UserOperationVm> prepareOneWithRealTimeFeeUpdates({
    required List<(BaseContract, String)> actions,
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
          channel,
          canceled,
        );

    return channel.stream;
  }

  void _keepRefreshingUserOpUntilCanceled(
    List<(BaseContract, String)> actions,
    String description,
    String functionSignature,
    BigInt? stakeSize,
    StreamController<UserOperationVm> channel,
    Completer canceled,
  ) async {
    var parseError = (String data) {
      var contractsAlreadyTriedParsing = <BaseContract>{};
      for (var (contract, _) in actions) {
        if (contractsAlreadyTriedParsing.contains(contract)) continue;
        try {
          contractsAlreadyTriedParsing.add(contract);
          return contract.parseError(data);
        } catch (e) {
          // Error: no matching error (argument="sighash", value="0x...", code=INVALID_ARGUMENT, version=abi/5.7.0)
        }
      }

      return null;
    };

    var addressAndCallDataPairs = actions.map((action) => (action.$1.address, action.$2)).toList();

    while (!canceled.isCompleted) {
      var result = await _createUnsignedFromBatch(actions: addressAndCallDataPairs);
      if (result.isLeft) {
        var error = result.left;
        if (error.isFurtherDecodable) {
          var errorDescription = parseError(error.message);
          if (errorDescription != null)
            error = UserOperationError(message: errorDescription.name);
          else
            error = UserOperationError();
        }

        channel.addError(error);
        return;
      }

      if (canceled.isCompleted) return;

      var userOp = result.right;
      userOp.parseError = parseError;

      channel.add(
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

  Future<Either<UserOperationError, UserOperation>> _createUnsignedFromBatch({
    required List<(String, String)> actions,
  }) async {
    assert(actions.isNotEmpty);

    var userOpBuilder = UserOperation.create();

    if (actions.length == 1) {
      userOpBuilder = userOpBuilder.action((actions.first.$1, actions.first.$2));
    } else {
      userOpBuilder = userOpBuilder.actions(actions);
    }

    try {
      return Right(await userOpBuilder.unsigned());
    } on UserOperationError catch (error) {
      logger.warning('[${error.code}] $error');
      return Left(error);
    }
  }

  Future<UserOperationError?> send(UserOperation approvedUserOp, {int confirmations = 1}) async {
    UserOperationError? error;
    String? userOpHash;
    try {
      var userOp = await UserOperation.createFrom(approvedUserOp).signed();
      logger.info('UserOp:\n$userOp');
      userOpHash = await _ethereumApiService.sendUserOperation(userOp);
    } on WalletActionDeclinedError catch (e) {
      logger.info(e);
      error = UserOperationError(message: e.message);
    } on GetCredentialError catch (e) {
      logger.info(e);
      error = UserOperationError(message: e.message);
    } on UserOperationError catch (e) {
      logger.warning('[${e.code}] $e');
      error = e;
      if (error.isFurtherDecodable) {
        var errorDescription = approvedUserOp.parseError!(error.message);
        if (errorDescription != null)
          error = UserOperationError(message: errorDescription.name);
        else
          error = UserOperationError();
      }
    }

    assert(userOpHash != null && error == null || userOpHash == null && error != null);

    if (error != null) return error;

    logger.info('UserOp Hash: $userOpHash');

    GetUserOperationReceiptRvm? receipt;
    do {
      await Future.delayed(const Duration(seconds: 2)); // @@TODO: Config.
      receipt = await _ethereumApiService.getUserOperationReceipt(userOpHash!);
    } while (receipt == null);

    while ((await _ethereumRpcProvider.provider.getBlockNumber() - receipt.receipt.blockNumber) < confirmations) {
      await Future.delayed(const Duration(seconds: 2)); // @@TODO: Config.
    }

    logger.info('Receipt:\n$receipt');

    var entryPointLogs = receipt.logs
        .where(
          // @@NOTE: Alchemy: l.address is all lower-case for some reason.
          (l) => l.address.toLowerCase() == _entryPointContract.address.toLowerCase(),
        )
        .toList();

    if (!receipt.success) {
      for (int i = 0; i < entryPointLogs.length; ++i) {
        var log = entryPointLogs[i];
        var logDescription = _entryPointContract.parseLog(log.topics, log.data);
        if (logDescription.name == _entryPointContract.userOperationRevertReasonEventName) {
          var revertReason = retrieveUserOpRevertReasonFromEvent(log.topics, log.data);
          var errorDescription = approvedUserOp.parseError!(revertReason);
          if (errorDescription != null) {
            logger.warning('UserOp Execution Failed. Reason: ${errorDescription.name}');
            return UserOperationError(message: errorDescription.name);
          } else {
            logger.warning('UserOp Execution Failed. Reason: $revertReason');
            return UserOperationError();
          }
        }
      }

      logger.warning('UserOp Execution Failed. Reason: Unspecified');
      return UserOperationError();
    }

    for (int i = 0; i < entryPointLogs.length; ++i) {
      var log = entryPointLogs[i];
      var logDescription = _entryPointContract.parseLog(log.topics, log.data);
      if (logDescription.name == _entryPointContract.userOperationEventName) {
        var status = retrieveUserOpStatusFromEvent(log.topics, log.data);
        logger.info(
          'UserOp succeeded: ${status.success}. Actual gas used: ${status.actualGasUsed}. Actual gas cost: ${status.actualGasCost}',
        );
      }
    }

    return null;
  }
}
