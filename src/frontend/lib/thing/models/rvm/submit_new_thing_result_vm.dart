import 'thing_vm.dart';

class SubmitNewThingResultVm {
  final ThingVm thing;
  final String signature;

  SubmitNewThingResultVm.fromMap(Map<String, dynamic> map)
      : thing = ThingVm.fromMap(map['thing']),
        signature = map['signature'];
}
