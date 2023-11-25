import 'thing_vm.dart';

class GetThingRvm {
  final ThingVm thing;
  final String? signature;

  GetThingRvm.fromMap(Map<String, dynamic> map)
      : thing = ThingVm.fromMap(map['thing']),
        signature = map['signature'];

  GetThingRvm({required this.thing, required this.signature});
}
