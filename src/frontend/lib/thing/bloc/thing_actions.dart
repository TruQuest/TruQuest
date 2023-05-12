import '../models/im/decision_im.dart';
import '../models/rvm/thing_vm.dart';
import '../../general/bloc/mixins.dart';
import 'thing_result_vm.dart';
import '../../general/contexts/document_context.dart';

abstract class ThingAction {}

abstract class ThingActionAwaitable<T extends ThingResultVm?>
    extends ThingAction with AwaitableResult<T> {}

class CreateNewThingDraft
    extends ThingActionAwaitable<CreateNewThingDraftFailureVm?> {
  final DocumentContext documentContext;

  CreateNewThingDraft({required this.documentContext});
}

class GetThing extends ThingAction {
  final String thingId;
  final bool subscribe;

  GetThing({required this.thingId, this.subscribe = false});
}

class SubmitNewThing extends ThingActionAwaitable<SubmitNewThingSuccessVm?> {
  final ThingVm thing;

  SubmitNewThing({required this.thing});
}

class FundThing extends ThingActionAwaitable<FundThingSuccessVm?> {
  final ThingVm thing;
  final String signature;

  FundThing({required this.thing, required this.signature});
}

class GetVerifierLotteryInfo extends ThingAction {
  final String thingId;

  GetVerifierLotteryInfo({required this.thingId});
}

class PreJoinLottery extends ThingActionAwaitable<PreJoinLotteryFailureVm?> {
  final String thingId;

  PreJoinLottery({required this.thingId});
}

class JoinLottery extends ThingActionAwaitable<JoinLotteryFailureVm?> {
  final String thingId;

  JoinLottery({required this.thingId});
}

class GetVerifierLotteryParticipants extends ThingAction {
  final String thingId;

  GetVerifierLotteryParticipants({required this.thingId});
}

class UnsubscribeFromThing extends ThingAction {
  final String thingId;

  UnsubscribeFromThing({required this.thingId});
}

class GetAcceptancePollInfo
    extends ThingActionAwaitable<GetAcceptancePollInfoSuccessVm> {
  final String thingId;

  GetAcceptancePollInfo({required this.thingId});
}

class CastVoteOffChain extends ThingActionAwaitable<CastVoteResultVm> {
  final String thingId;
  final DecisionIm decision;
  final String reason;

  CastVoteOffChain({
    required this.thingId,
    required this.decision,
    required this.reason,
  });
}

class CastVoteOnChain extends ThingActionAwaitable<CastVoteResultVm> {
  final String thingId;
  final DecisionIm decision;
  final String reason;

  CastVoteOnChain({
    required this.thingId,
    required this.decision,
    required this.reason,
  });
}

class GetVerifiers extends ThingAction {
  final String thingId;

  GetVerifiers({required this.thingId});
}

class GetSettlementProposalsList extends ThingAction {
  final String thingId;

  GetSettlementProposalsList({required this.thingId});
}

class Watch extends ThingAction {
  final String thingId;
  final bool markedAsWatched;

  Watch({
    required this.thingId,
    required this.markedAsWatched,
  });
}
