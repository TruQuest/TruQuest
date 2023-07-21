import '../models/rvm/get_thing_rvm.dart';
import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';
import '../../general/errors/error.dart';

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

class FundThingFailureVm extends ThingResultVm {
  final Error error;

  FundThingFailureVm({required this.error});
}

class GetVerifierLotteryInfoSuccessVm extends ThingResultVm {
  final String? userId;
  final int? initBlock;
  final int durationBlocks;
  final bool? alreadyJoined;

  GetVerifierLotteryInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
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

  GetAcceptancePollInfoSuccessVm({
    required this.userId,
    required this.initBlock,
    required this.durationBlocks,
    required this.userIndexInThingVerifiersArray,
  });
}

class JoinLotteryFailureVm extends ThingResultVm {
  final Error error;

  JoinLotteryFailureVm({required this.error});
}

class CastVoteOffChainFailureVm extends ThingResultVm {
  final Error error;

  CastVoteOffChainFailureVm({required this.error});
}

class CastVoteOnChainFailureVm extends ThingResultVm {
  final Error error;

  CastVoteOnChainFailureVm({required this.error});
}
