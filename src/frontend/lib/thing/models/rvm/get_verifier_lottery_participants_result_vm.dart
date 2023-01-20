import 'verifier_lottery_participant_entry_vm.dart';

class GetVerifierLotteryParticipantsResultVm {
  final String thingId;
  final List<VerifierLotteryParticipantEntryVm> entries;

  GetVerifierLotteryParticipantsResultVm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        entries = List.unmodifiable(
          (map['entries'] as List<dynamic>).map(
            (submap) => VerifierLotteryParticipantEntryVm.fromMap(submap),
          ),
        );
}
