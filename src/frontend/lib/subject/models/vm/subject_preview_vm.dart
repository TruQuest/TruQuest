import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../general/models/vm/tag_vm.dart';
import 'subject_type_vm.dart';

class SubjectPreviewVm {
  final String id;
  final DateTime submittedAt;
  final String name;
  final SubjectTypeVm type;
  final String croppedImageIpfsCid;
  final int settledThingsCount;
  final int avgScore;
  final List<TagVm> tags;

  String get submittedAtFormatted => DateFormat.yMEd().format(submittedAt);

  IconData get typeIcon => type == SubjectTypeVm.person ? Icons.person : Icons.groups;

  SubjectPreviewVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        submittedAt = DateTime.fromMillisecondsSinceEpoch(map['submittedAt']),
        name = map['name'],
        type = SubjectTypeVm.values[map['type']],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        settledThingsCount = map['settledThingsCount'],
        avgScore = map['avgScore'],
        tags = List.unmodifiable(
          (map['tags'] as List<dynamic>).map((submap) => TagVm.fromMap(submap)),
        );
}
