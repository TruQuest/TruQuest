import '../../../general/models/rvm/tag_vm.dart';
import 'evidence_vm.dart';
import 'thing_state_vm.dart';

class ThingVm {
  final String id;
  final ThingStateVm state;
  final String title;
  final String details;
  final String? imageIpfsCid;
  final String? croppedImageIpfsCid;
  final String submitterId;
  final String subjectId;
  final List<EvidenceVm> evidence;
  final List<TagVm> tags;

  final bool? fundedAwaitingConfirmation;

  ThingVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        state = ThingStateVm.values[map['state']],
        title = map['title'],
        details = map['details'],
        imageIpfsCid = map['imageIpfsCid'],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        subjectId = map['subjectId'],
        evidence = List.unmodifiable(
          (map['evidence'] as List<dynamic>)
              .map((submap) => EvidenceVm.fromMap(submap)),
        ),
        tags = List.unmodifiable(
          (map['tags'] as List<dynamic>).map((submap) => TagVm.fromMap(submap)),
        ),
        fundedAwaitingConfirmation = null;

  ThingVm._({
    required this.id,
    required this.state,
    required this.title,
    required this.details,
    required this.imageIpfsCid,
    required this.croppedImageIpfsCid,
    required this.submitterId,
    required this.subjectId,
    required this.evidence,
    required this.tags,
    required this.fundedAwaitingConfirmation,
  });

  ThingVm copyWith({ThingStateVm? state, bool? fundedAwaitingConfirmation}) {
    return ThingVm._(
      id: id,
      state: state ?? this.state,
      title: title,
      details: details,
      imageIpfsCid: imageIpfsCid,
      croppedImageIpfsCid: croppedImageIpfsCid,
      submitterId: submitterId,
      subjectId: subjectId,
      evidence: evidence,
      tags: tags,
      fundedAwaitingConfirmation:
          fundedAwaitingConfirmation ?? this.fundedAwaitingConfirmation,
    );
  }
}
