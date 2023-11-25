import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../settlement/models/vm/settlement_proposal_state_vm.dart';
import '../../../settlement/models/vm/verdict_vm.dart';

class SettlementProposalPreviewVm {
  final String id;
  final SettlementProposalStateVm state;
  final String title;
  final VerdictVm verdict;
  final String? croppedImageIpfsCid;
  final String submitterId;
  final DateTime? displayedTimestamp;

  String get displayedTimestampFormatted =>
      displayedTimestamp != null ? DateFormat('EEE, M/d/y').format(displayedTimestamp!) : 'Not submitted yet';

  IconData get stateIcon {
    switch (state) {
      case SettlementProposalStateVm.draft:
        return Icons.drive_file_rename_outline_outlined;
      case SettlementProposalStateVm.awaitingFunding:
        return Icons.attach_money;
      case SettlementProposalStateVm.fundedAndVerifierLotteryInitiated:
        return Icons.people;
      case SettlementProposalStateVm.verifierLotteryFailed:
        return Icons.emoji_people;
      case SettlementProposalStateVm.verifiersSelectedAndPollInitiated:
        return Icons.poll_outlined;
      case SettlementProposalStateVm.consensusNotReached:
        return Icons.drag_handle;
      case SettlementProposalStateVm.declined:
        return Icons.thumb_down;
      case SettlementProposalStateVm.accepted:
        return Icons.thumb_up;
    }
  }

  SettlementProposalPreviewVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        state = SettlementProposalStateVm.values[map['state']],
        title = map['title'],
        verdict = VerdictVm.values[map['verdict']],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        displayedTimestamp =
            map['displayedTimestamp'] != null ? DateTime.fromMillisecondsSinceEpoch(map['displayedTimestamp']) : null;
}
