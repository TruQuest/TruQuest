import '../models/rvm/thing_vm.dart';
import '../../general/bloc/mixins.dart';
import 'thing_result_vm.dart';
import '../../general/contexts/document_context.dart';

abstract class ThingAction {}

abstract class ThingActionAwaitable<T extends ThingResultVm?>
    extends ThingAction with AwaitableResult<T> {}

class CreateNewThingDraft
    extends ThingActionAwaitable<CreateNewThingDraftResultVm> {
  final DocumentContext documentContext;

  CreateNewThingDraft({required this.documentContext});
}

class GetThing extends ThingAction {
  final String thingId;

  GetThing({required this.thingId});
}

class SubmitNewThing extends ThingAction {
  final ThingVm thing;

  SubmitNewThing({required this.thing});
}

class FundThing extends ThingAction {
  final ThingVm thing;
  final String signature;

  FundThing({required this.thing, required this.signature});
}

class GetThingLotteryInfo extends ThingAction {
  final String thingId;

  GetThingLotteryInfo({required this.thingId});
}

class PreJoinLottery extends ThingAction {
  final String thingId;

  PreJoinLottery({required this.thingId});
}

class JoinLottery extends ThingAction {
  final String thingId;

  JoinLottery({required this.thingId});
}
