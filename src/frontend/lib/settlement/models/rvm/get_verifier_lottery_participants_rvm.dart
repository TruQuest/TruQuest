import '../../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';

class GetVerifierLotteryParticipantsRvm {
  final String proposalId;
  final List<VerifierLotteryParticipantEntryVm> entries;

  GetVerifierLotteryParticipantsRvm.fromMap(Map<String, dynamic> map)
      : proposalId = map['proposalId'],
        entries = List.unmodifiable(
          (map['entries'] as List<dynamic>).map(
            (submap) => VerifierLotteryParticipantEntryVm.fromMap(submap),
          ),
        );
}
