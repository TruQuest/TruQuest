import '../../general/bloc/mixins.dart';
import 'ethereum_result_vm.dart';

abstract class EthereumAction {
  const EthereumAction();
}

abstract class EthereumActionAwaitable<T extends EthereumResultVm?>
    extends EthereumAction with AwaitableResult<T> {}

class SelectWallet extends EthereumActionAwaitable<SelectWalletFailureVm?> {
  final String walletName;

  SelectWallet({required this.walletName});
}

class SwitchEthereumChain extends EthereumAction {
  const SwitchEthereumChain();
}

class ConnectEthereumAccount
    extends EthereumActionAwaitable<ConnectEthereumAccountSuccessVm?> {}

class WatchTruthserum extends EthereumAction {
  const WatchTruthserum();
}
