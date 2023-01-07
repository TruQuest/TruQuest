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

abstract class SignAuthMessageResultVm extends EthereumResultVm {}

class SignAuthMessageSuccessVm extends SignAuthMessageResultVm {
  final String account;
  final String signature;

  SignAuthMessageSuccessVm({required this.account, required this.signature});
}

class SignAuthMessageFailureVm extends SignAuthMessageResultVm {}
