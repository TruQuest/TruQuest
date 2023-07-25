import '../../general/errors/error.dart';

abstract class UserResultVm {}

class SelectThirdPartyWalletSuccessVm extends UserResultVm {
  final bool shouldRequestAccounts;

  SelectThirdPartyWalletSuccessVm({required this.shouldRequestAccounts});
}

class ConnectAccountSuccessVm extends UserResultVm {
  final String? walletConnectUri;

  ConnectAccountSuccessVm({required this.walletConnectUri});
}

class GenerateMnemonicSuccessVm extends UserResultVm {
  final String mnemonic;

  GenerateMnemonicSuccessVm({required this.mnemonic});
}

class CreateAndSaveEncryptedLocalWalletSuccessVm extends UserResultVm {}

class AddEmailSuccessVm extends UserResultVm {}

class ConfirmEmailSuccessVm extends UserResultVm {}

class UnlockWalletSuccessVm extends UserResultVm {}

class AddAccountFailureVm extends UserResultVm {
  final Error error;

  AddAccountFailureVm({required this.error});
}

class SwitchAccountSuccessVm extends UserResultVm {}

class ApproveFundsUsageFailureVm extends UserResultVm {
  final Error error;

  ApproveFundsUsageFailureVm({required this.error});
}
