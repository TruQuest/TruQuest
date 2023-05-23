import 'package:intl/intl.dart';

import '../../../general/models/rvm/tag_vm.dart';
import 'evidence_vm.dart';
import 'thing_state_vm.dart';

class ThingVm {
  final String id;
  final ThingStateVm state;
  final DateTime? submittedAt;
  final String title;
  final String details;
  final String? imageIpfsCid;
  final String? croppedImageIpfsCid;
  final String submitterId;
  final String subjectId;
  final String subjectName;
  final String subjectCroppedImageIpfsCid;
  final DateTime? settledAt;
  final String? acceptedSettlementProposalId;
  final List<EvidenceVm> evidence;
  final List<TagVm> tags;
  final int? subjectAvgScore;
  final bool watched;

  final bool? fundedAwaitingConfirmation;

  String get submittedAtFormatted =>
      DateFormat.yMMMMd('en_US').format(submittedAt!);

  String get submitterIdShort =>
      submitterId.substring(0, 6) +
      '...' +
      submitterId.substring(submitterId.length - 4, submitterId.length);

  ThingVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        state = ThingStateVm.values[map['state']],
        submittedAt = map['submittedAt'] != null
            ? DateTime.fromMillisecondsSinceEpoch(map['submittedAt'])
            : null,
        title = map['title'],
        details = map['details'],
        imageIpfsCid = map['imageIpfsCid'],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        subjectId = map['subjectId'],
        subjectName = map['subjectName'],
        subjectCroppedImageIpfsCid = map['subjectCroppedImageIpfsCid'],
        settledAt = map['settledAt'] != null
            ? DateTime.fromMillisecondsSinceEpoch(map['settledAt'])
            : null,
        acceptedSettlementProposalId = map['acceptedSettlementProposalId'],
        evidence = List.unmodifiable(
          (map['evidence'] as List<dynamic>)
              .map((submap) => EvidenceVm.fromMap(submap)),
        ),
        tags = List.unmodifiable(
          (map['tags'] as List<dynamic>).map((submap) => TagVm.fromMap(submap)),
        ),
        subjectAvgScore = map['subjectAvgScore'],
        watched = map['watched'],
        fundedAwaitingConfirmation = null;

  ThingVm._({
    required this.id,
    required this.state,
    required this.submittedAt,
    required this.title,
    required this.details,
    required this.imageIpfsCid,
    required this.croppedImageIpfsCid,
    required this.submitterId,
    required this.subjectId,
    required this.subjectName,
    required this.subjectCroppedImageIpfsCid,
    required this.settledAt,
    required this.acceptedSettlementProposalId,
    required this.evidence,
    required this.tags,
    required this.subjectAvgScore,
    required this.watched,
    required this.fundedAwaitingConfirmation,
  });

  ThingVm copyWith({ThingStateVm? state, bool? fundedAwaitingConfirmation}) {
    return ThingVm._(
      id: id,
      state: state ?? this.state,
      submittedAt: submittedAt,
      title: title,
      details: details,
      imageIpfsCid: imageIpfsCid,
      croppedImageIpfsCid: croppedImageIpfsCid,
      submitterId: submitterId,
      subjectId: subjectId,
      subjectName: subjectName,
      subjectCroppedImageIpfsCid: subjectCroppedImageIpfsCid,
      settledAt: settledAt,
      acceptedSettlementProposalId: acceptedSettlementProposalId,
      evidence: evidence,
      tags: tags,
      subjectAvgScore: subjectAvgScore,
      watched: watched,
      fundedAwaitingConfirmation:
          fundedAwaitingConfirmation ?? this.fundedAwaitingConfirmation,
    );
  }
}
