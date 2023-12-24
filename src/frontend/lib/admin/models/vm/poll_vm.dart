import 'verifier_vm.dart';

class PollVm {
  final int initBlockNumber;
  final List<VerifierVm> verifiers;

  PollVm.fromMap(Map<String, dynamic> map)
      : initBlockNumber = map['initBlockNumber'],
        verifiers = List.unmodifiable(
          (map['verifiers'] as List<dynamic>).map((submap) => VerifierVm.fromMap(submap)),
        );
}
