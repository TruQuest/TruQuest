import 'thing_vm.dart';

class GetThingResultVm {
  final ThingVm thing;
  final String? signature;

  GetThingResultVm.fromMap(Map<String, dynamic> map)
      : thing = ThingVm.fromMap(map['thing']),
        signature = map['signature'];

  GetThingResultVm({required this.thing, required this.signature});
}
