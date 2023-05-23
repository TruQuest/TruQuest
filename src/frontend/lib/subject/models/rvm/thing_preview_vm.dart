import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../general/models/rvm/tag_vm.dart';
import '../../../settlement/models/rvm/verdict_vm.dart';
import '../../../thing/models/rvm/thing_state_vm.dart';

class ThingPreviewVm {
  final String id;
  final ThingStateVm state;
  final String title;
  final String? croppedImageIpfsCid;
  final DateTime? displayedTimestamp;
  final VerdictVm? verdict;
  final List<TagVm> tags;

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
      case ThingStateVm.declined:
        return Icons.remove_done;
      case ThingStateVm.awaitingSettlement:
        return Icons.pending_outlined;
      case ThingStateVm.settled:
        return Icons.handshake;
    }
  }

  String get displayedTimestampFormatted => displayedTimestamp != null
      ? DateFormat('EEE, M/d/y').format(displayedTimestamp!)
      : 'Not submitted yet';

  Color get verdictColor {
    switch (verdict!) {
      case VerdictVm.delivered:
        return Color(0xff10A19D);
      case VerdictVm.guessItCounts:
        return Color(0xff088395);
      case VerdictVm.aintGoodEnough:
        return Color(0xff8BACAA);
      case VerdictVm.motionNotAction:
        return Color(0xffF99B7D);
      case VerdictVm.noEffortWhatsoever:
        return Color(0xffE76161);
      case VerdictVm.asGoodAsMaliciousIntent:
        return Color(0xffB04759);
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
            map['verdict'] != null ? VerdictVm.values[map['verdict']] : null,
        tags = List.unmodifiable(
          (map['tags'] as List<dynamic>).map((submap) => TagVm.fromMap(submap)),
        );
}
