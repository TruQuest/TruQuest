import 'dart:math';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import 'decision_vm.dart';

class VoteVm {
  static final _colors = [
    Color(0xff72efdd),
    Color(0xff64dfdf),
    Color(0xff56cfe1),
    Color(0xff48bfe3),
    Color(0xff4ea8de),
    Color(0xff5390d9),
  ];

  static final _random = Random();

  final String userId;
  final String walletAddress;
  final int? castedAtMs;
  final int? l1BlockNumber;
  final int? blockNumber;
  final DecisionVm? decision;
  final String? reason;
  final String? ipfsCid;
  final String? txnHash;

  Color get cardColor => _colors[_random.nextInt(_colors.length)];

  String get decisionString => decision?.getString() ?? 'Hidden';

  String get onOrOffChain => blockNumber != null ? 'Onchain' : 'Offchain';

  String get castedVoteAt =>
      l1BlockNumber?.toString() ?? DateFormat.jm().format(DateTime.fromMillisecondsSinceEpoch(castedAtMs!));

  String get walletAddressShort =>
      '${walletAddress.substring(0, 6)}..${walletAddress.substring(walletAddress.length - 4)}';

  VoteVm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        walletAddress = map['walletAddress'],
        castedAtMs = map['castedAtMs'],
        l1BlockNumber = map['l1BlockNumber'],
        blockNumber = map['blockNumber'],
        decision = map['decision'] != null ? DecisionVm.values[map['decision']] : null,
        reason = map['reason'],
        ipfsCid = map['ipfsCid'],
        txnHash = map['txnHash'] {
    assert(castedAtMs != null && l1BlockNumber == null || castedAtMs == null && l1BlockNumber != null);
  }
}
