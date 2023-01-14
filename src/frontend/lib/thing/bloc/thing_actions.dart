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

class SubmitNewThing extends ThingActionAwaitable<SubmitNewThingSuccessVm?> {
  final String thingId;

  SubmitNewThing({required this.thingId});
}
