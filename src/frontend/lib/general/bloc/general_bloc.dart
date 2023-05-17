import 'bloc.dart';
import 'general_actions.dart';
import 'general_result_vm.dart';
import '../services/general_service.dart';

class GeneralBloc extends Bloc<GeneralAction> {
  final GeneralService _generalService;

  GeneralBloc(this._generalService) {
    actionChannel.stream.listen((action) {
      if (action is GetTags) {
        _getTags(action);
      }
    });
  }

  void _getTags(GetTags action) async {
    var tags = await _generalService.getTags();
    action.complete(GetTagsSuccessVm(tags: tags));
  }
}
