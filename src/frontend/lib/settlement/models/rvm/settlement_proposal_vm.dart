import 'settlement_proposal_state_vm.dart';
import 'supporting_evidence_vm.dart';
import 'verdict_vm.dart';

class SettlementProposalVm {
  final String id;
  final String thingId;
  final SettlementProposalStateVm state;
  final String title;
  final VerdictVm verdict;
  final String details;
  final String? imageIpfsCid;
  final String? croppedImageIpfsCid;
  final String submitterId;
  final List<SupportingEvidenceVm> evidence;

  final bool? canBeFunded;

  SettlementProposalVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        thingId = map['thingId'],
        state = SettlementProposalStateVm.values[map['state']],
        title = map['title'],
        verdict = VerdictVm.values[map['verdict']],
        details = map['details'],
        imageIpfsCid = map['imageIpfsCid'],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        evidence = List.unmodifiable(
          (map['evidence'] as List<dynamic>)
              .map((submap) => SupportingEvidenceVm.fromMap(submap)),
        ),
        canBeFunded = null;

  SettlementProposalVm._({
    required this.id,
    required this.thingId,
    required this.state,
    required this.title,
    required this.verdict,
    required this.details,
    required this.imageIpfsCid,
    required this.croppedImageIpfsCid,
    required this.submitterId,
    required this.evidence,
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
        title: title,
        verdict: verdict,
        details: details,
        imageIpfsCid: imageIpfsCid,
        croppedImageIpfsCid: croppedImageIpfsCid,
        submitterId: submitterId,
        evidence: evidence,
        canBeFunded: canBeFunded ?? this.canBeFunded,
      );
}
