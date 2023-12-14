import 'dart:math';

import 'package:flutter/material.dart';
import 'package:dio/dio.dart';
import 'package:bot_toast/bot_toast.dart';
import 'package:convert/convert.dart';
import 'package:intl/intl.dart';
import 'package:uuid/uuid.dart';

import '../errors/api_error.dart';
import '../errors/connection_error.dart';
import '../errors/error.dart';
import '../../ethereum/models/vm/user_operation_vm.dart';
import '../../ethereum_js_interop.dart';
import '../../user/errors/get_credential_error.dart';
import '../../user/errors/local_key_share_not_present_error.dart';
import '../errors/failed_multi_stage_execution_error.dart';
import '../errors/forbidden_error.dart';
import '../errors/handle_error.dart';
import '../errors/invalid_authentication_token_error.dart';
import '../errors/server_error.dart';
import '../errors/unhandled_error.dart';
import '../errors/validation_error.dart';
import '../../ethereum/models/vm/wallet_connect_uri_vm.dart';
import '../../ethereum/errors/wallet_action_declined_error.dart';
import '../../ethereum/errors/user_operation_error.dart';
import '../../ethereum/models/im/user_operation.dart';
import '../contexts/multi_stage_operation_context.dart';
import '../errors/insufficient_balance_error.dart';
import '../widgets/qr_code_dialog.dart';
import '../widgets/scan_key_share_dialog.dart';
import '../widgets/user_operation_dialog.dart';
import 'logger.dart';

Error wrapDioException(DioException ex) {
  switch (ex.type) {
    case DioExceptionType.connectionError:
    case DioExceptionType.connectionTimeout:
    case DioExceptionType.sendTimeout:
    case DioExceptionType.receiveTimeout:
      logger.warning('Connection error', ex);
      return ConnectionError();
    case DioExceptionType.badCertificate:
      logger.warning('Connection error', ex);
      return ServerError(message: 'Bad certificate');
    case DioExceptionType.badResponse:
      var response = ex.response!;
      var statusCode = response.statusCode!;
      if (statusCode >= 500) {
        logger.warning('Server error', ex);
        return ServerError();
      }

      switch (statusCode) {
        case 400:
          var error = response.data['error'];
          if (error.containsKey('isUnhandled')) {
            logger.warning('Server unhandled error (traceId: ${error['traceId']}): ${error['message']}');
            return UnhandledError(error['message'], error['traceId']);
          }

          logger.info('Server handled error: ${error['message']}');
          return HandleError(error['message']);
        case 401:
          var errorMessage = response.data['error']['message'];
          // if (errorMessage.contains('token expired at')) {
          //   return AuthenticationTokenExpiredError();
          // }
          logger.warning('Auth error: $errorMessage');
          return InvalidAuthenticationTokenError(errorMessage);
        case 403:
          logger.warning('Auth error: forbidden');
          return ForbiddenError();
      }
    default:
  }

  logger.error('Unknown error', ex);
  return ApiError();
}

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

  String trimZeros() {
    if (indexOf('.') == -1) return this;

    int index = length;
    while (this[index - 1] == '0') --index;

    return substring(0, this[index - 1] == '.' ? index - 1 : index);
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

// @@??: Do I really need 2 separate functions for flow?
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
      // @@NOTE: WalletActionDeclinedError and GetCredentialError are wrapped into this.
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithoutErrors = false;
    } else if (stageResult is FailedMultiStageExecutionError) {
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
    } else if (stageResult is GetCredentialError) {
      BotToast.showText(text: stageResult.message);
      proceededTilTheEndWithoutErrors = false;
    } else if (stageResult is LocalKeyShareNotPresentError) {
      if (stageResult.scanRequestId != null) {
        await showDialog(
          context: context,
          builder: (_) => ScanKeyShareDialog(scanRequestId: stageResult.scanRequestId!),
        );
        ctx.scanQrCodeTask.complete();
      } else {
        BotToast.showText(text: stageResult.message);
        proceededTilTheEndWithoutErrors = false;
      }
    } else if (stageResult is FailedMultiStageExecutionError) {
      proceededTilTheEndWithoutErrors = false;
    } else {
      throw UnimplementedError();
    }
  }

  return proceededTilTheEndWithoutErrors;
}
