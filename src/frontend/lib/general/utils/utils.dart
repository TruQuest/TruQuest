import 'dart:math';

import 'package:flutter/material.dart';

import '../../ethereum/models/im/user_operation.dart';
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
