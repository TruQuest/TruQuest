import '../models/im/decision_im.dart';
import '../models/rvm/thing_vm.dart';
import '../../general/bloc/mixins.dart';
import 'thing_result_vm.dart';
import '../../general/contexts/document_context.dart';

abstract class ThingAction {
  bool get mustValidate => false;
  List<String>? validate() => null;
}

abstract class ThingActionAwaitable<T extends ThingResultVm?>
    extends ThingAction with AwaitableResult<T> {}

class CreateNewThingDraft
    extends ThingActionAwaitable<CreateNewThingDraftFailureVm?> {
  final DocumentContext documentContext;

  CreateNewThingDraft({required this.documentContext});

  @override
  bool get mustValidate => true;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (documentContext.subjectId == null) {
      errors ??= [];
      errors.add('• Subject Id is not set');
    }
    if (documentContext.nameOrTitle == null ||
        documentContext.nameOrTitle!.length < 3) {
      errors ??= [];
      errors.add('• Title should be at least 3 characters long');
    }
    if (documentContext.details!.isEmpty) {
      errors ??= [];
      errors.add('• Details are not specified');
    }
    if (documentContext.evidence.isEmpty) {
      errors ??= [];
      errors.add('• Must provide evidence');
    }

    return errors;
  }
}

class GetThing extends ThingAction {
  final String thingId;

  GetThing({required this.thingId});
}

class SubmitNewThing extends ThingActionAwaitable<SubmitNewThingFailureVm?> {
  final ThingVm thing;

  SubmitNewThing({required this.thing});
}

class FundThing extends ThingActionAwaitable<FundThingFailureVm?> {
  final ThingVm thing;
  final String signature;

  FundThing({required this.thing, required this.signature});
}

class GetVerifierLotteryInfo extends ThingAction {
  final String thingId;

  GetVerifierLotteryInfo({required this.thingId});
}

class JoinLottery extends ThingActionAwaitable<JoinLotteryFailureVm?> {
  final String thingId;

  JoinLottery({required this.thingId});
}

class GetVerifierLotteryParticipants extends ThingAction {
  final String thingId;

  GetVerifierLotteryParticipants({required this.thingId});
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
