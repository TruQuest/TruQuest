import '../../../general/models/rvm/verifier_vm.dart';

class GetVerifiersRvm {
  final String proposalId;
  final List<VerifierVm> verifiers;

  GetVerifiersRvm.fromMap(Map<String, dynamic> map)
      : proposalId = map['proposalId'],
        verifiers = List.unmodifiable(
          (map['verifiers'] as List<dynamic>)
              .map((submap) => VerifierVm.fromMap(submap)),
        );
}
