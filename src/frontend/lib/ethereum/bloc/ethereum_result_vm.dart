abstract class EthereumResultVm {}

class SwitchEthereumChainSuccessVm extends EthereumResultVm {
  final int chainId;
  final bool shouldOfferToSwitchChain;

  SwitchEthereumChainSuccessVm({
    required this.chainId,
    required this.shouldOfferToSwitchChain,
  });
}

class SwitchEthereumChainFailureVm extends EthereumResultVm {}

class ConnectEthereumAccountFailureVm extends EthereumResultVm {}

abstract class SignAuthMessageResultVm extends EthereumResultVm {}

class SignAuthMessageSuccessVm extends SignAuthMessageResultVm {
  final String signature;

  SignAuthMessageSuccessVm({required this.signature});
}

class SignAuthMessageFailureVm extends SignAuthMessageResultVm {}
