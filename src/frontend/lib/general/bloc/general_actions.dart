import 'actions.dart';
import 'general_result_vm.dart';
import 'mixins.dart';

abstract class GeneralAction extends Action {
  const GeneralAction();
}

abstract class GeneralActionAwaitable<T extends GeneralResultVm>
    extends GeneralAction with AwaitableResult<T> {}

class GetTags extends GeneralActionAwaitable<GetTagsSuccessVm> {}
