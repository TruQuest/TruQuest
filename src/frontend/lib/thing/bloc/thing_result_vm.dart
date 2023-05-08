import '../../general/models/rvm/verifier_lottery_participant_entry_vm.dart';

abstract class ThingResultVm {}

class CreateNewThingDraftFailureVm extends ThingResultVm {}

class SubmitNewThingSuccessVm extends ThingResultVm {}

class FundThingSuccessVm extends ThingResultVm {}

class GetVerifierLotteryInfoSuccessVm extends ThingResultVm {
  final int? initBlock;
  final int durationBlocks;
  final int latestBlockNumber;
  final bool? alreadyPreJoined;
  final bool? alreadyJoined;

  GetVerifierLotteryInfoSuccessVm({
    required this.initBlock,
    required this.durationBlocks,
    required this.latestBlockNumber,
    required this.alreadyPreJoined,
    required this.alreadyJoined,
  });
}

class GetVerifierLotteryParticipantsSuccessVm extends ThingResultVm {
  final List<VerifierLotteryParticipantEntryVm> entries;

  GetVerifierLotteryParticipantsSuccessVm({required this.entries});
}

class GetAcceptancePollInfoSuccessVm extends ThingResultVm {
  final int? initBlock;
  final int durationBlocks;
  final bool? isDesignatedVerifier;
  final int latestBlockNumber;

  GetAcceptancePollInfoSuccessVm({
    required this.initBlock,
    required this.durationBlocks,
    required this.isDesignatedVerifier,
    required this.latestBlockNumber,
  });
}

class PreJoinLotteryFailureVm extends ThingResultVm {}

class JoinLotteryFailureVm extends ThingResultVm {}

class CastVoteResultVm extends ThingResultVm {}
