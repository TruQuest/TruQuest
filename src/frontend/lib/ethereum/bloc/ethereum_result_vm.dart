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

abstract class SignSignUpMessageResultVm extends EthereumResultVm {}

class SignSignUpMessageSuccessVm extends SignSignUpMessageResultVm {
  final String account;
  final String signature;

  SignSignUpMessageSuccessVm({required this.account, required this.signature});
}

class SignSignUpMessageFailureVm extends SignSignUpMessageResultVm {}
