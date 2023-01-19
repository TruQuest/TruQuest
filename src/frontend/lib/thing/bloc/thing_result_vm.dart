abstract class ThingResultVm {}

abstract class CreateNewThingDraftResultVm extends ThingResultVm {}

class CreateNewThingDraftSuccessVm extends CreateNewThingDraftResultVm {}

class CreateNewThingDraftFailureVm extends CreateNewThingDraftResultVm {}

class GetThingLotteryInfoSuccessVm extends ThingResultVm {
  final int initBlock;
  final int durationBlocks;
  final int latestBlockNumber;
  final bool alreadyPreJoined;
  final bool alreadyJoined;

  GetThingLotteryInfoSuccessVm({
    required this.initBlock,
    required this.durationBlocks,
    required this.latestBlockNumber,
    required this.alreadyPreJoined,
    required this.alreadyJoined,
  });
}
