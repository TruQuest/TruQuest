import 'package:intl/intl.dart';

import 'thing_preview_vm.dart';
import '../../../general/models/rvm/tag_vm.dart';
import 'subject_type_vm.dart';

class SubjectVm {
  final String id;
  final DateTime submittedAt;
  final String name;
  final String details;
  final SubjectTypeVm type;
  final String imageIpfsCid;
  final String croppedImageIpfsCid;
  final String submitterId;
  final String submitterWalletAddress;
  final int settledThingsCount;
  final int avgScore;
  final List<ThingPreviewVm> latestSettledThings;
  final List<ThingPreviewVm> latestUnsettledThings;
  final List<TagVm> tags;

  String get submittedAtFormatted => DateFormat.yMMMMd('en_US').format(submittedAt);

  String get submitterWalletAddressShort =>
      submitterWalletAddress.substring(0, 6) +
      '...' +
      submitterWalletAddress.substring(submitterWalletAddress.length - 4, submitterWalletAddress.length);

  SubjectVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        submittedAt = DateTime.fromMillisecondsSinceEpoch(map['submittedAt']),
        name = map['name'],
        details = map['details'],
        type = SubjectTypeVm.values[map['type']],
        imageIpfsCid = map['imageIpfsCid'],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        submitterWalletAddress = map['submitterWalletAddress'],
        settledThingsCount = map['settledThingsCount'],
        avgScore = map['avgScore'],
        latestSettledThings = List.unmodifiable(
          (map['latestSettledThings'] as List<dynamic>).map((submap) => ThingPreviewVm.fromMap(submap)),
        ),
        latestUnsettledThings = List.unmodifiable(
          (map['latestUnsettledThings'] as List<dynamic>).map((submap) => ThingPreviewVm.fromMap(submap)),
        ),
        tags = List.unmodifiable(
          (map['tags'] as List<dynamic>).map((submap) => TagVm.fromMap(submap)),
        );
}
