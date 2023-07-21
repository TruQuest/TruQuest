import 'dart:math';

import 'package:flutter/material.dart';

import '../widgets/unlock_wallet_dialog.dart';

double degreesToRadians(double degrees) => (pi / 180) * degrees;

extension BigIntExtension on BigInt {
  String toHex() => '0x' + toRadixString(16);
}

Future<bool> showUnlockWalletDialog(BuildContext context) async {
  var unlocked = await showDialog<bool>(
    context: context,
    builder: (_) => UnlockWalletDialog(),
  );

  return unlocked != null && unlocked;
}
