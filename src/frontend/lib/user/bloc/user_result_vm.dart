import '../../ethereum/models/vm/smart_wallet.dart';
import '../../general/errors/error.dart';

abstract class UserResultVm {}

class GenerateMnemonicSuccessVm extends UserResultVm {
  final String mnemonic;

  GenerateMnemonicSuccessVm({required this.mnemonic});
}

class CreateSmartWalletFromMnemonicSuccessVm extends UserResultVm {
  final SmartWallet wallet;

  CreateSmartWalletFromMnemonicSuccessVm({required this.wallet});
}

class CreateAndSaveEncryptedSmartWalletSuccessVm extends UserResultVm {}

class SignInWithEthereumSuccessVm extends UserResultVm {}

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

class DepositFundsFailureVm extends UserResultVm {
  final Error error;

  DepositFundsFailureVm({required this.error});
}
