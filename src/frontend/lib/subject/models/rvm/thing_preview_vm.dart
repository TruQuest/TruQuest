import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../settlement/models/rvm/verdict_vm.dart';
import '../../../thing/models/rvm/thing_state_vm.dart';

class ThingPreviewVm {
  final String id;
  final ThingStateVm state;
  final String title;
  final String? croppedImageIpfsCid;
  final DateTime? displayedTimestamp;
  final VerdictVm? verdict;

  IconData get stateIcon {
    switch (state) {
      case ThingStateVm.draft:
        return Icons.drive_file_rename_outline_outlined;
      case ThingStateVm.awaitingFunding:
        return Icons.attach_money;
      case ThingStateVm.fundedAndVerifierLotteryInitiated:
        return Icons.people;
      case ThingStateVm.verifiersSelectedAndPollInitiated:
        return Icons.poll_outlined;
      case ThingStateVm.awaitingSettlement:
        return Icons.pending_outlined;
      case ThingStateVm.settled:
        return Icons.handshake;
    }
  }

  String get displayedTimestampFormatted =>
      DateFormat('EEE, M/d/y').format(displayedTimestamp!);

  Color get verdictColor {
    switch (verdict!) {
      case VerdictVm.delivered:
        return Colors.green;
      case VerdictVm.guessItCounts:
        return Color.fromARGB(255, 136, 193, 93);
      case VerdictVm.aintGoodEnough:
        return Color.fromARGB(255, 238, 255, 83);
      case VerdictVm.motionNotAction:
        return Color.fromARGB(255, 255, 209, 84);
      case VerdictVm.noEffortWhatsoever:
        return Color.fromARGB(255, 255, 108, 108);
      case VerdictVm.asGoodAsMaliciousIntent:
        return Color.fromARGB(255, 145, 0, 0);
    }
  }

  ThingPreviewVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        state = ThingStateVm.values[map['state']],
        title = map['title'],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        displayedTimestamp = map['displayedTimestamp'] != null
            ? DateTime.fromMillisecondsSinceEpoch(map['displayedTimestamp'])
            : null,
        verdict =
            map['verdict'] != null ? VerdictVm.values[map['verdict']] : null;
}
