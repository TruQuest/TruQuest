import '../../general/bloc/mixins.dart';
import 'ethereum_result_vm.dart';

abstract class EthereumAction {}

abstract class EthereumActionAwaitable<T extends EthereumResultVm?>
    extends EthereumAction with AwaitableResult<T> {}

class SwitchEthereumChain extends EthereumAction {}

class ConnectEthereumAccount
    extends EthereumActionAwaitable<ConnectEthereumAccountFailureVm?> {}

class ApproveFundsUsage
    extends EthereumActionAwaitable<ApproveFundsUsageFailureVm?> {
  final int amount;

  ApproveFundsUsage({required this.amount});
}

class DepositFunds extends EthereumAction {
  final int amount;

  DepositFunds({required this.amount});
}
