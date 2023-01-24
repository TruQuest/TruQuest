import 'vote_vm.dart';

class VerifierVm {
  final String verifierId;
  final String username;
  final VoteVm? vote;

  VerifierVm.fromMap(Map<String, dynamic> map)
      : verifierId = map['verifierId'],
        username = map['userName'],
        vote = map['vote'] != null ? VoteVm.fromMap(map['vote']) : null;
}
