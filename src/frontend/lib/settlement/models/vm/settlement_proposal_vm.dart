import 'package:intl/intl.dart';

import '../../../general/models/vm/evidence_vm.dart';
import 'settlement_proposal_state_vm.dart';
import 'verdict_vm.dart';

class SettlementProposalVm {
  final String id;
  final String thingId;
  final SettlementProposalStateVm state;
  final DateTime? submittedAt;
  final String title;
  final VerdictVm verdict;
  final String details;
  final String? imageIpfsCid;
  final String? croppedImageIpfsCid;
  final String submitterId;
  final String submitterWalletAddress;
  final DateTime? assessmentPronouncedAt;
  final String subjectName;
  final String thingTitle;
  final String? thingCroppedImageIpfsCid;
  final List<EvidenceVm> evidence;
  final bool watched;

  final bool? canBeFunded;

  String get submittedAtFormatted => DateFormat.yMMMMd('en_US').format(submittedAt!);

  String get submitterWalletAddressShort =>
      submitterWalletAddress.substring(0, 6) +
      '...' +
      submitterWalletAddress.substring(submitterWalletAddress.length - 4, submitterWalletAddress.length);

  bool isSubmitter(String? userId) => userId == submitterId;

  SettlementProposalVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        thingId = map['thingId'],
        state = SettlementProposalStateVm.values[map['state']],
        submittedAt = map['submittedAt'] != null ? DateTime.fromMillisecondsSinceEpoch(map['submittedAt']) : null,
        title = map['title'],
        verdict = VerdictVm.values[map['verdict']],
        details = map['details'],
        imageIpfsCid = map['imageIpfsCid'],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        submitterWalletAddress = map['submitterWalletAddress'],
        assessmentPronouncedAt = map['assessmentPronouncedAt'] != null
            ? DateTime.fromMillisecondsSinceEpoch(map['assessmentPronouncedAt'])
            : null,
        subjectName = map['subjectName'],
        thingTitle = map['thingTitle'],
        thingCroppedImageIpfsCid = map['thingCroppedImageIpfsCid'],
        evidence = List.unmodifiable(
          (map['evidence'] as List<dynamic>).map((submap) => EvidenceVm.fromMap(submap)),
        ),
        watched = map['watched'],
        canBeFunded = null;

  SettlementProposalVm._({
    required this.id,
    required this.thingId,
    required this.state,
    required this.submittedAt,
    required this.title,
    required this.verdict,
    required this.details,
    required this.imageIpfsCid,
    required this.croppedImageIpfsCid,
    required this.submitterId,
    required this.submitterWalletAddress,
    required this.assessmentPronouncedAt,
    required this.subjectName,
    required this.thingTitle,
    required this.thingCroppedImageIpfsCid,
    required this.evidence,
    required this.watched,
    required this.canBeFunded,
  });

  SettlementProposalVm copyWith({
    SettlementProposalStateVm? state,
    bool? canBeFunded,
  }) =>
      SettlementProposalVm._(
        id: id,
        thingId: thingId,
        state: state ?? this.state,
        submittedAt: submittedAt,
        title: title,
        verdict: verdict,
        details: details,
        imageIpfsCid: imageIpfsCid,
        croppedImageIpfsCid: croppedImageIpfsCid,
        submitterId: submitterId,
        submitterWalletAddress: submitterWalletAddress,
        assessmentPronouncedAt: assessmentPronouncedAt,
        subjectName: subjectName,
        thingTitle: thingTitle,
        thingCroppedImageIpfsCid: thingCroppedImageIpfsCid,
        evidence: evidence,
        watched: watched,
        canBeFunded: canBeFunded ?? this.canBeFunded,
      );
}
