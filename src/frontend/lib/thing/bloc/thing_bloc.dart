import 'thing_result_vm.dart';
import '../../general/bloc/bloc.dart';
import '../services/thing_service.dart';
import 'thing_actions.dart';

class ThingBloc extends Bloc<ThingAction> {
  final ThingService _thingService;

  ThingBloc(this._thingService) {
    actionChannel.stream.listen((action) {
      if (action is CreateNewThingDraft) {
        _createNewThingDraft(action);
      } else if (action is SubmitNewThing) {
        _submitNewThing(action);
      }
    });
  }

  @override
  void dispose({ThingAction? cleanupAction}) {
    // TODO: implement dispose
  }

  void _createNewThingDraft(CreateNewThingDraft action) async {
    await _thingService.createNewThingDraft(action.documentContext);
    action.complete(CreateNewThingDraftSuccessVm());
  }

  void _submitNewThing(SubmitNewThing action) async {
    await _thingService.submitNewThing(action.thingId);
    action.complete(SubmitNewThingSuccessVm());
  }
}
