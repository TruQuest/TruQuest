import 'verifier_vm.dart';

class GetVerifiersRvm {
  final String thingId;
  final List<VerifierVm> verifiers;

  GetVerifiersRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        verifiers = List.unmodifiable(
          (map['verifiers'] as List<dynamic>)
              .map((submap) => VerifierVm.fromMap(submap)),
        );
}
