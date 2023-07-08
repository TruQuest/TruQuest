import '../../general/bloc/mixins.dart';
import 'ethereum_result_vm.dart';

abstract class EthereumAction {
  const EthereumAction();
}

abstract class EthereumActionAwaitable<T extends EthereumResultVm?>
    extends EthereumAction with AwaitableResult<T> {}

class SelectWallet extends EthereumAction {
  final String walletName;

  const SelectWallet({required this.walletName});
}

class SwitchEthereumChain extends EthereumAction {
  const SwitchEthereumChain();
}

class ConnectEthereumAccount
    extends EthereumActionAwaitable<ConnectEthereumAccountFailureVm?> {}

class WatchTruthserum extends EthereumAction {
  const WatchTruthserum();
}

class ApproveFundsUsage
    extends EthereumActionAwaitable<ApproveFundsUsageFailureVm?> {
  final int amount;

  ApproveFundsUsage({required this.amount});
}

class DepositFunds extends EthereumAction {
  final int amount;

  const DepositFunds({required this.amount});
}
