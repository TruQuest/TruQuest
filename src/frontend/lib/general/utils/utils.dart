import 'dart:math';

import 'package:flutter/material.dart';
import 'package:bot_toast/bot_toast.dart';

import '../errors/validation_error.dart';
import '../../ethereum/models/vm/wallet_connect_uri_vm.dart';
import '../../ethereum/errors/wallet_action_declined_error.dart';
import '../../ethereum/errors/user_operation_error.dart';
import '../../ethereum/models/im/user_operation.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../contexts/multi_stage_operation_context.dart';
import '../errors/insufficient_balance_error.dart';
import '../widgets/qr_code_dialog.dart';
import '../widgets/unlock_wallet_dialog.dart';
import '../widgets/user_operation_dialog.dart';

double degreesToRadians(double degrees) => (pi / 180) * degrees;

extension BigIntExtension on BigInt {
  String toHex() => '0x' + toRadixString(16);
}

Future<bool> showUnlockWalletDialog(BuildContext context) async {
  var unlocked = await showDialog<bool>(
    context: context,
    builder: (_) => const UnlockWalletDialog(),
  );

  return unlocked != null && unlocked;
}

Future<UserOperation?> showUserOpDialog(
  BuildContext context,
  Stream<UserOperation> stream,
) =>
    showDialog<UserOperation?>(
      context: context,
      builder: (_) => UserOperationDialog(stream: stream),
    );

Future<bool> multiStageAction(
  BuildContext context,
  Stream<Object> Function(MultiStageOperationContext ctx) action,
) async {
  var proceededTilTheEndWithNoErrors = true;
  var ctx = MultiStageOperationContext();
  await for (var stageResult in action(ctx)) {
    if (stageResult is ValidationError) {
      proceededTilTheEndWithNoErrors = false;
    } else if (stageResult is WalletLockedError) {
      bool unlocked = false;
      if (context.mounted) {
        unlocked = await showUnlockWalletDialog(context);
      }
      ctx.unlockWalletTask.complete(unlocked);
      if (!unlocked) {
        proceededTilTheEndWithNoErrors = false;
      }
    } else if (stageResult is InsufficientBalanceError) {
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithNoErrors = false;
    } else if (stageResult is Stream<UserOperation>) {
      UserOperation? userOp;
      if (context.mounted) {
        userOp = await showUserOpDialog(context, stageResult);
      }
      ctx.approveUserOpTask.complete(userOp);
      if (userOp == null) {
        proceededTilTheEndWithNoErrors = false;
      }
    } else if (stageResult is UserOperationError) {
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithNoErrors = false;
    }
  }

  return proceededTilTheEndWithNoErrors;
}

Future<bool> multiStageOffChainAction(
  BuildContext context,
  Stream<Object> Function(MultiStageOperationContext ctx) action,
) async {
  var proceededTilTheEndWithNoErrors = true;
  var ctx = MultiStageOperationContext();
  await for (var stageResult in action(ctx)) {
    if (stageResult is ValidationError) {
      proceededTilTheEndWithNoErrors = false;
    } else if (stageResult is WalletLockedError) {
      bool unlocked = false;
      if (context.mounted) {
        unlocked = await showUnlockWalletDialog(context);
      }
      ctx.unlockWalletTask.complete(unlocked);
      if (!unlocked) {
        proceededTilTheEndWithNoErrors = false;
      }
    } else if (stageResult is WalletActionDeclinedError) {
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithNoErrors = false;
    } else if (stageResult is WalletConnectUriVm) {
      showDialog(
        context: context,
        builder: (_) => QrCodeDialog(uri: stageResult.uri),
      );
    }
  }

  return proceededTilTheEndWithNoErrors;
}
