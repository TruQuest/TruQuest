abstract class EthereumResultVm {
  const EthereumResultVm();
}

class SelectWalletFailureVm extends EthereumResultVm {
  const SelectWalletFailureVm();
}

class ConnectEthereumAccountSuccessVm extends EthereumResultVm {
  final String? walletConnectUri;

  const ConnectEthereumAccountSuccessVm({required this.walletConnectUri});
}

class ApproveFundsUsageFailureVm extends EthereumResultVm {
  const ApproveFundsUsageFailureVm();
}
