import 'thing_preview_vm.dart';

class GetThingsListRvm {
  final String subjectId;
  final List<ThingPreviewVm> things;

  GetThingsListRvm.fromMap(Map<String, dynamic> map)
      : subjectId = map['subjectId'],
        things = List.unmodifiable(
          (map['things'] as List<dynamic>)
              .map((submap) => ThingPreviewVm.fromMap(submap)),
        );
}
