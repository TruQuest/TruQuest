abstract class EthereumResultVm {}

class ConnectEthereumAccountFailureVm extends EthereumResultVm {}

abstract class SignAuthMessageResultVm extends EthereumResultVm {}

class SignAuthMessageSuccessVm extends SignAuthMessageResultVm {
  final String signature;

  SignAuthMessageSuccessVm({required this.signature});
}

class SignAuthMessageFailureVm extends SignAuthMessageResultVm {}
