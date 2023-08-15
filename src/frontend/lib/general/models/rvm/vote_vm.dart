import 'package:intl/intl.dart';

import 'decision_vm.dart';

class VoteVm {
  final String userId;
  final int? castedAtMs;
  final int? blockNumber;

  VoteVm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        castedAtMs = map['castedAtMs'],
        blockNumber = map['blockNumber'];
}

class Vote2Vm {
  final String userId;
  final int? castedAtMs;
  final int? l1BlockNumber;
  final int? blockNumber;
  final DecisionVm? decision;
  final String? reason;
  final String? ipfsCid;
  final String? txnHash;

  String get castedVoteAt =>
      l1BlockNumber?.toString() ?? DateFormat.jm().format(DateTime.fromMillisecondsSinceEpoch(castedAtMs!));

  Vote2Vm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
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
