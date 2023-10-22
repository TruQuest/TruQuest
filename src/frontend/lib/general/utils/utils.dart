import 'dart:math';

import 'package:flutter/material.dart';
import 'package:bot_toast/bot_toast.dart';
import 'package:convert/convert.dart';
import 'package:intl/intl.dart';
import 'package:uuid/uuid.dart';

import '../../ethereum/models/vm/user_operation_vm.dart';
import '../../ethereum_js_interop.dart';
import '../errors/validation_error.dart';
import '../../ethereum/models/vm/wallet_connect_uri_vm.dart';
import '../../ethereum/errors/wallet_action_declined_error.dart';
import '../../ethereum/errors/user_operation_error.dart';
import '../../ethereum/models/im/user_operation.dart';
import '../contexts/multi_stage_operation_context.dart';
import '../errors/insufficient_balance_error.dart';
import '../widgets/qr_code_dialog.dart';
import '../widgets/user_operation_dialog.dart';

double degreesToRadians(double degrees) => (pi / 180) * degrees;

extension BigIntExtension on BigInt {
  String toHex() => '0x' + toRadixString(16);

  String toStringWithSpaces() {
    var s = toString().split('').reversed.toList(); // works fine since string contains only digits
    var spaceCount = (s.length - 1) ~/ 3;
    for (int i = 0; i < spaceCount; ++i) {
      var index = (i + 1) * 3 + i;
      s.insert(index, ' ');
    }

    return s.reversed.join();
  }
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

extension MapExtension<K, V> on Map<K, V> {
  V? getValueOrNull(K key) => containsKey(key) ? this[key] : null;
}

String getFixedLengthAmount(BigInt amount, String tokenSymbol, [int length = 3]) {
  var balanceString = formatUnits(
    BigNumber.from(amount.toString()),
    tokenSymbol == 'ETH' ? 'ether' : 'gwei',
  );
  var balanceStringSplit = balanceString.split('.');
  var decimals = balanceStringSplit.length == 1 ? ''.padRight(length, '0') : '';
  if (decimals == '') {
    decimals = balanceStringSplit.last;
    if (decimals.length < length) {
      decimals = decimals.padRight(length, '0');
    } else if (decimals.length > length) {
      decimals = decimals.substring(0, length);
    }
  }

  return '${balanceStringSplit.first}.$decimals';
}

String getMinLengthAmount(BigInt amount, String tokenSymbol, [int length = 3]) {
  var balanceString = formatUnits(
    BigNumber.from(amount.toString()),
    tokenSymbol == 'ETH' ? 'ether' : 'gwei',
  );
  var balanceStringSplit = balanceString.split('.');
  var decimals = balanceStringSplit.length == 1 ? ''.padRight(length, '0') : '';
  if (decimals == '') {
    decimals = balanceStringSplit.last;
    if (decimals.length < length) {
      decimals = decimals.padRight(length, '0');
    }
  }

  return '${balanceStringSplit.first}.$decimals';
}

ThemeData getThemeDataForSteppers(BuildContext context) => ThemeData(
      brightness: Brightness.dark,
      colorScheme: Theme.of(context).colorScheme.copyWith(
            brightness: Brightness.dark,
            secondary: const Color(0xffF8F9FA),
          ),
    );

Future<UserOperation?> _showUserOpDialog(
  BuildContext context,
  Stream<UserOperationVm> stream,
) =>
    showDialog<UserOperation?>(
      context: context,
      builder: (_) => UserOperationDialog(stream: stream),
    );

Future<bool> multiStageFlow(
  BuildContext context,
  Stream<Object> Function(MultiStageOperationContext ctx) action,
) async {
  var proceededTilTheEndWithoutErrors = true;
  var ctx = MultiStageOperationContext();
  await for (var stageResult in action(ctx)) {
    if (stageResult is ValidationError) {
      proceededTilTheEndWithoutErrors = false;
    } else if (stageResult is InsufficientBalanceError) {
      // @@TODO: Get rid of BotToast here. Show all messages through ToastMessenger.
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithoutErrors = false;
    } else if (stageResult is Stream<UserOperationVm>) {
      UserOperation? userOp;
      if (context.mounted) {
        userOp = await _showUserOpDialog(context, stageResult);
      }
      ctx.approveUserOpTask.complete(userOp);
      if (userOp == null) proceededTilTheEndWithoutErrors = false;
    } else if (stageResult is UserOperationError) {
      // @@NOTE: WalletActionDeclinedError is wrapped into this.
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithoutErrors = false;
    } else {
      throw UnimplementedError();
    }
  }

  return proceededTilTheEndWithoutErrors;
}

Future<bool> multiStageOffChainFlow(
  BuildContext context,
  Stream<Object> Function(MultiStageOperationContext ctx) action,
) async {
  var proceededTilTheEndWithoutErrors = true;
  var ctx = MultiStageOperationContext();
  await for (var stageResult in action(ctx)) {
    if (stageResult is ValidationError) {
      proceededTilTheEndWithoutErrors = false;
    } else if (stageResult is WalletActionDeclinedError) {
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithoutErrors = false;
    } else if (stageResult is WalletConnectUriVm) {
      showDialog(
        context: context,
        builder: (_) => QrCodeDialog(uri: stageResult.uri),
      );
    } else {
      throw UnimplementedError();
    }
  }

  return proceededTilTheEndWithoutErrors;
}
