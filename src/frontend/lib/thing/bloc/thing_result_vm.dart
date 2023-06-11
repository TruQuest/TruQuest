import '../models/rvm/get_thing_rvm.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';

abstract class ThingResultVm {}

class CreateNewThingDraftFailureVm extends ThingResultVm {}

abstract class GetThingResultVm extends ThingResultVm {}

class GetThingSuccessVm extends GetThingResultVm {
  final GetThingRvm result;

  GetThingSuccessVm({required this.result});
}

class GetThingFailureVm extends GetThingResultVm {
  final String message;

  GetThingFailureVm({required this.message});
}

class SubmitNewThingFailureVm extends ThingResultVm {}

class FundThingFailureVm extends ThingResultVm {}

class GetVerifierLotteryInfoSuccessVm extends ThingResultVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int latestL1BlockNumber;
  final bool? alreadyJoined;

  GetVerifierLotteryInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.latestL1BlockNumber,
    required this.alreadyJoined,
  });
}

class GetVerifierLotteryParticipantsSuccessVm extends ThingResultVm {
  final List<VerifierLotteryParticipantEntryVm> entries;

  GetVerifierLotteryParticipantsSuccessVm({required this.entries});
}

class GetAcceptancePollInfoSuccessVm extends ThingResultVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final int userIndexInThingVerifiersArray;
  final int latestL1BlockNumber;

  GetAcceptancePollInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInThingVerifiersArray,
    required this.latestL1BlockNumber,
  });
}

class JoinLotteryFailureVm extends ThingResultVm {}

class CastVoteResultVm extends ThingResultVm {}
