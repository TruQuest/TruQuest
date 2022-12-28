import "../../general/bloc/mixins.dart";
import "ethereum_result_vm.dart";

abstract class EthereumAction {}

abstract class EthereumActionAwaitable<T extends EthereumResultVm?>
    extends EthereumAction with AwaitableResult<T> {}

class ConnectEthereumAccount
    extends EthereumActionAwaitable<ConnectEthereumAccountFailureVm?> {}

class SignAuthMessage extends EthereumActionAwaitable<SignAuthMessageResultVm> {
  final String username;

  SignAuthMessage({required this.username});
}
