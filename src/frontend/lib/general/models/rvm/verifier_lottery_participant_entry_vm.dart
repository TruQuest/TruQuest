import 'package:flutter/material.dart';

class VerifierLotteryParticipantEntryVm {
  static final _coldColors = [
    Color(0xff72efdd),
    Color(0xff64dfdf),
    Color(0xff56cfe1),
    Color(0xff48bfe3),
    Color(0xff4ea8de),
    Color(0xff5390d9),
  ];

  static final _warmColors = [
    Color(0xffF28266),
    Color(0xffF17063),
    Color(0xffEF5D60),
    Color(0xffEE4F64),
    Color(0xffEC4067),
    Color(0xffD9376D),
  ];

  final int l1BlockNumber;
  final String txnHash;
  final String userId;
  final String walletAddress;
  final String userData;
  final int? nonce;
  final bool isWinner;

  String get nonceString => nonce?.toString() ?? '*';

  String get commitment => userData.substring(0, 15) + '..';

  String get walletAddressShort =>
      '${walletAddress.substring(0, 6)}..${walletAddress.substring(walletAddress.length - 4)}';

  Color get coldCardColor => _coldColors[int.parse(walletAddress.substring(0, 6)) % _coldColors.length];
  Color get warmCardColor => _warmColors[int.parse(walletAddress.substring(0, 6)) % _warmColors.length];

  VerifierLotteryParticipantEntryVm.fromMap(Map<String, dynamic> map)
      : l1BlockNumber = map['l1BlockNumber'],
        txnHash = map['txnHash'],
        userId = map['userId'],
        walletAddress = map['walletAddress'],
        userData = map['userData'],
        nonce = map['nonce'],
        isWinner = map.containsKey('isWinner'); // either true or absent
}
