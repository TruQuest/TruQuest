import 'dart:math';

import 'package:flutter/material.dart';

class VerifierLotteryParticipantEntryVm {
  static final _colors = [
    Color(0xff72efdd),
    Color(0xff64dfdf),
    Color(0xff56cfe1),
    Color(0xff48bfe3),
    Color(0xff4ea8de),
    Color(0xff5390d9),
  ];

  static final _random = Random();

  final int l1BlockNumber;
  final String txnHash;
  final String userId;
  final String userData;
  final int? nonce;
  final bool isWinner;

  String get nonceString => nonce?.toString() ?? '*';

  String get commitment => userData.substring(0, 15) + '..';

  String get userIdShort => '${userId.substring(0, 6)}..${userId.substring(userId.length - 4)}';

  Color get cardColor => _colors[_random.nextInt(_colors.length)];

  VerifierLotteryParticipantEntryVm.fromMap(Map<String, dynamic> map)
      : l1BlockNumber = map['l1BlockNumber'],
        txnHash = map['txnHash'],
        userId = map['userId'],
        userData = map['userData'],
        nonce = map['nonce'],
        isWinner = map.containsKey('isWinner'); // either true or absent
}
