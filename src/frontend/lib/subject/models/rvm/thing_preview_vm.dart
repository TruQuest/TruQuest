import '../../../thing/models/rvm/thing_state_vm.dart';

class ThingPreviewVm {
  final String id;
  final ThingStateVm state;
  final String title;
  final String croppedImageIpfsCid;
  final DateTime sortedByDate;

  ThingPreviewVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        state = ThingStateVm.values[map['state']],
        title = map['title'],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        sortedByDate = DateTime.fromMillisecondsSinceEpoch(map['sortedByDate']);
}
