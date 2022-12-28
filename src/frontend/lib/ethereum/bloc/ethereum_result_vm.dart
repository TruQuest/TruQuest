abstract class EthereumResultVm {}

abstract class SwitchEthereumChainResultVm extends EthereumResultVm {}

class SwitchEthereumChainSuccessVm extends SwitchEthereumChainResultVm {
  final int chainId;
  final bool shouldRefreshPage;

  SwitchEthereumChainSuccessVm({
    required this.chainId,
    required this.shouldRefreshPage,
  });
}

class ConnectEthereumAccountFailureVm extends EthereumResultVm {}

abstract class SignAuthMessageResultVm extends EthereumResultVm {}

class SignAuthMessageSuccessVm extends SignAuthMessageResultVm {
  final String signature;

  SignAuthMessageSuccessVm({required this.signature});
}

class SignAuthMessageFailureVm extends SignAuthMessageResultVm {}
