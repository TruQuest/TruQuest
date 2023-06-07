import '../../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';

class GetVerifierLotteryParticipantsRvm {
  final String thingId;
  final List<VerifierLotteryParticipantEntryVm> entries;

  GetVerifierLotteryParticipantsRvm({
    required this.thingId,
    required this.entries,
  });

  GetVerifierLotteryParticipantsRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        entries = List.unmodifiable(
          (map['entries'] as List<dynamic>).map(
            (submap) => VerifierLotteryParticipantEntryVm.fromMap(submap),
          ),
        );
}
