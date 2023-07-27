import 'dart:math';

import 'package:flutter/material.dart';
import 'package:bot_toast/bot_toast.dart';
import 'package:convert/convert.dart';
import 'package:intl/intl.dart';
import 'package:uuid/uuid.dart';

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

extension BoolExtension on bool? {
  bool get isTrue => this ?? false;
}

extension StringExtension on String {
  bool get isValidUuid => Uuid.isValidUUID(fromString: this);

  String toSolInputFormat({bool prefix = true}) {
    var thingIdBytes = Uuid.parse(this, validate: false);
    int b0 = thingIdBytes[0];
    int b1 = thingIdBytes[1];
    int b2 = thingIdBytes[2];
    int b3 = thingIdBytes[3];
    thingIdBytes[0] = b3;
    thingIdBytes[1] = b2;
    thingIdBytes[2] = b1;
    thingIdBytes[3] = b0;

    int b4 = thingIdBytes[4];
    int b5 = thingIdBytes[5];
    thingIdBytes[4] = b5;
    thingIdBytes[5] = b4;

    int b6 = thingIdBytes[6];
    int b7 = thingIdBytes[7];
    thingIdBytes[6] = b7;
    thingIdBytes[7] = b6;

    return (prefix ? '0x' : '') + hex.encode(thingIdBytes);
  }
}

extension DateTimeExtension on DateTime {
  String getString() {
    var s = DateFormat('yyyy-MM-dd HH:mm:ss').format(this);
    Duration offset = timeZoneOffset;
    int hours = offset.inHours > 0 ? offset.inHours : 1;

    if (!offset.isNegative) {
      s += '+' +
          offset.inHours.toString().padLeft(2, '0') +
          ':' +
          (offset.inMinutes % (hours * 60)).toString().padLeft(2, '0');
    } else {
      s += '-' +
          (-offset.inHours).toString().padLeft(2, '0') +
          ':' +
          (offset.inMinutes % (hours * 60)).toString().padLeft(2, '0');
    }

    return s;
  }
}

extension IterableExtension<E> on Iterable<E> {
  Iterable<T> mapIndexed<T>(T Function(E e, int i) f) {
    var i = 0;
    return map((e) => f(e, i++));
  }
}

ThemeData getThemeDataForSteppers(BuildContext context) => ThemeData(
      brightness: Brightness.dark,
      colorScheme: Theme.of(context).colorScheme.copyWith(
            brightness: Brightness.dark,
            secondary: const Color(0xffF8F9FA),
          ),
    );

Future<bool> _showUnlockWalletDialog(BuildContext context) async {
  var unlocked = await showDialog<bool>(
    context: context,
    builder: (_) => const UnlockWalletDialog(),
  );

  return unlocked.isTrue;
}

Future<UserOperation?> _showUserOpDialog(
  BuildContext context,
  Stream<UserOperation> stream,
) =>
    showDialog<UserOperation?>(
      context: context,
      builder: (_) => UserOperationDialog(stream: stream),
    );

Future<bool> multiStageFlow(
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
        unlocked = await _showUnlockWalletDialog(context);
      }
      ctx.unlockWalletTask.complete(unlocked);
      if (!unlocked) {
        proceededTilTheEndWithNoErrors = false;
      }
    } else if (stageResult is InsufficientBalanceError) {
      // @@TODO: Get rid of BotToast here. Show all messages through ToastMessenger.
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithNoErrors = false;
    } else if (stageResult is Stream<UserOperation>) {
      UserOperation? userOp;
      if (context.mounted) {
        userOp = await _showUserOpDialog(context, stageResult);
      }
      ctx.approveUserOpTask.complete(userOp);
      if (userOp == null) {
        proceededTilTheEndWithNoErrors = false;
      }
    } else if (stageResult is UserOperationError) {
      // @@NOTE: WalletActionDeclinedError is wrapped into this.
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithNoErrors = false;
    }
  }

  return proceededTilTheEndWithNoErrors;
}

Future<bool> multiStageOffChainFlow(
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
        unlocked = await _showUnlockWalletDialog(context);
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
