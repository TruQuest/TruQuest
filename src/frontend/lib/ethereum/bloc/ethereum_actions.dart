import '../../general/bloc/actions.dart';
import '../../general/bloc/mixins.dart';
import 'ethereum_result_vm.dart';

abstract class EthereumAction extends Action {
  const EthereumAction();
}

abstract class EthereumActionAwaitable<T extends EthereumResultVm?>
    extends EthereumAction with AwaitableResult<T> {}

class WatchTruthserum extends EthereumAction {
  const WatchTruthserum();
}
