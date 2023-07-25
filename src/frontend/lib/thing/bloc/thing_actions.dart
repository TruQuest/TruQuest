import '../models/rvm/thing_vm.dart';
import '../../general/bloc/mixins.dart';
import 'thing_result_vm.dart';
import '../../general/contexts/document_context.dart';

abstract class ThingAction {
  bool get mustValidate => false;
  List<String>? validate() => null;

  const ThingAction();
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
      errors.add('Subject Id is not set');
    }
    if (documentContext.nameOrTitle == null ||
        documentContext.nameOrTitle!.length < 3) {
      errors ??= [];
      errors.add('Title should be at least 3 characters long');
    }
    if (documentContext.details!.isEmpty) {
      errors ??= [];
      errors.add('Details are not specified');
    }
    if (documentContext.evidence.isEmpty) {
      errors ??= [];
      errors.add('Must provide evidence');
    }

    return errors;
  }
}

class GetThing extends ThingAction {
  final String thingId;

  const GetThing({required this.thingId});
}

class SubmitNewThing extends ThingActionAwaitable<SubmitNewThingFailureVm?> {
  final ThingVm thing;

  SubmitNewThing({required this.thing});
}

class GetVerifierLotteryInfo extends ThingAction {
  final String thingId;

  const GetVerifierLotteryInfo({required this.thingId});
}

class GetVerifierLotteryParticipants extends ThingAction {
  final String thingId;

  const GetVerifierLotteryParticipants({required this.thingId});
}

class GetAcceptancePollInfo
    extends ThingActionAwaitable<GetAcceptancePollInfoSuccessVm> {
  final String thingId;

  GetAcceptancePollInfo({required this.thingId});
}

class GetVerifiers extends ThingAction {
  final String thingId;

  const GetVerifiers({required this.thingId});
}

class GetSettlementProposalsList extends ThingAction {
  final String thingId;

  const GetSettlementProposalsList({required this.thingId});
}

class Watch extends ThingAction {
  final String thingId;
  final bool markedAsWatched;

  const Watch({
    required this.thingId,
    required this.markedAsWatched,
  });
}
