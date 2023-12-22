import 'thing_vm.dart';

class SubjectVm {
  final String id;
  final String name;
  final List<ThingVm> things;

  SubjectVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        name = map['name'],
        things = List.unmodifiable(
          (map['things'] as List<dynamic>).map((submap) => ThingVm.fromMap(submap)),
        );
}
