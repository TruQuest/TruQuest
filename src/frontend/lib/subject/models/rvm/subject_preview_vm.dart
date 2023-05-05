import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../general/models/rvm/tag_vm.dart';
import 'subject_type_vm.dart';

class SubjectPreviewVm {
  final String id;
  final DateTime submittedAt;
  final String name;
  final SubjectTypeVm type;
  final String croppedImageIpfsCid;
  final String submitterId;
  final int? settledThingsCount;
  final int? avgScore;
  final List<TagVm> tags;

  String get submittedAtFormatted => DateFormat.yMEd().format(submittedAt);

  String get submitterIdShort =>
      submitterId.substring(0, 6) +
      '...' +
      submitterId.substring(submitterId.length - 4, submitterId.length);

  IconData get typeIcon =>
      type == SubjectTypeVm.person ? Icons.person : Icons.groups;

  SubjectPreviewVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        submittedAt = DateTime.fromMillisecondsSinceEpoch(map['submittedAt']),
        name = map['name'],
        type = SubjectTypeVm.values[map['type']],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        settledThingsCount = map['settledThingsCount'],
        avgScore = map['avgScore'],
        tags = List.unmodifiable(
          (map['tags'] as List<dynamic>).map((submap) => TagVm.fromMap(submap)),
        );
}
