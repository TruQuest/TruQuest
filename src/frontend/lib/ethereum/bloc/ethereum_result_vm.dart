abstract class EthereumResultVm {}

abstract class SwitchEthereumChainResultVm extends EthereumResultVm {}

class SwitchEthereumChainSuccessVm extends SwitchEthereumChainResultVm {
  final int chainId;
  final bool shouldOfferToSwitchChain;

  SwitchEthereumChainSuccessVm({
    required this.chainId,
    required this.shouldOfferToSwitchChain,
  });
}

class SwitchEthereumChainFailureVm extends SwitchEthereumChainResultVm {}

class ConnectEthereumAccountFailureVm extends EthereumResultVm {}

class ApproveFundsUsageFailureVm extends EthereumResultVm {}
