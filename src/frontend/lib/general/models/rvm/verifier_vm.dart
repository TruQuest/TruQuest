import 'package:intl/intl.dart';

import 'vote_vm.dart';

class VerifierVm {
  final String verifierId;
  final String username;
  final VoteVm? vote;

  String get castedVoteAt =>
      vote?.blockNumber?.toString() ??
      (vote?.castedAtMs != null
          ? DateFormat.jm()
              .format(DateTime.fromMillisecondsSinceEpoch(vote!.castedAtMs!))
          : '*');

  VerifierVm.fromMap(Map<String, dynamic> map)
      : verifierId = map['verifierId'],
        username = map['userName'],
        vote = map['vote'] != null ? VoteVm.fromMap(map['vote']) : null;
}
