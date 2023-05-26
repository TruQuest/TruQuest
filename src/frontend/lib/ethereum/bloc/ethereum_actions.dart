import '../../general/bloc/mixins.dart';
import 'ethereum_result_vm.dart';

abstract class EthereumAction {}

abstract class EthereumActionAwaitable<T extends EthereumResultVm?>
    extends EthereumAction with AwaitableResult<T> {}

class SwitchEthereumChain
    extends EthereumActionAwaitable<SwitchEthereumChainFailureVm?> {}

class ConnectEthereumAccount
    extends EthereumActionAwaitable<ConnectEthereumAccountFailureVm?> {}
