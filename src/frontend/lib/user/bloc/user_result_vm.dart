import '../models/vm/user_vm.dart';
import '../../ethereum/models/vm/smart_wallet.dart';
import '../../general/errors/error.dart';

abstract class UserResultVm {}

class LoadCurrentUserSuccessVm extends UserResultVm {
  final UserVm user;

  LoadCurrentUserSuccessVm({required this.user});
}

class CreateSmartWalletSuccessVm extends UserResultVm {
  final SmartWallet wallet;

  CreateSmartWalletSuccessVm({required this.wallet});
}

class CreateSmartWalletFromMnemonicSuccessVm extends UserResultVm {
  final SmartWallet wallet;

  CreateSmartWalletFromMnemonicSuccessVm({required this.wallet});
}

class EncryptAndSaveSmartWalletSuccessVm extends UserResultVm {}

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
