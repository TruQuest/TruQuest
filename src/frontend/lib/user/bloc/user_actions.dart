import 'user_result_vm.dart';
import '../../general/bloc/mixins.dart';

abstract class UserAction {
  const UserAction();
}

abstract class UserActionAwaitable<T extends UserResultVm?> extends UserAction
    with AwaitableResult<T> {}

class GenerateMnemonic
    extends UserActionAwaitable<GenerateMnemonicSuccessVm?> {}

class CreateAndSaveEncryptedSmartWallet
    extends UserActionAwaitable<CreateAndSaveEncryptedSmartWalletSuccessVm?> {
  final String mnemonic;
  final String password;

  CreateAndSaveEncryptedSmartWallet({
    required this.mnemonic,
    required this.password,
  });
}

class SignInWithEthereum
    extends UserActionAwaitable<SignInWithEthereumSuccessVm?> {
  final String password;

  SignInWithEthereum({required this.password});
}

class AddEmail extends UserActionAwaitable<AddEmailSuccessVm?> {
  final String email;

  AddEmail({required this.email});
}

class ConfirmEmail extends UserActionAwaitable<ConfirmEmailSuccessVm?> {
  final String confirmationToken;

  ConfirmEmail({required this.confirmationToken});
}

class UnlockWallet extends UserActionAwaitable<UnlockWalletSuccessVm?> {
  final String password;

  UnlockWallet({required this.password});
}

class AddAccount extends UserActionAwaitable<AddAccountFailureVm?> {}

class SwitchAccount extends UserActionAwaitable<SwitchAccountSuccessVm> {
  final String walletAddress;

  SwitchAccount({required this.walletAddress});
}

class ApproveFundsUsage
    extends UserActionAwaitable<ApproveFundsUsageFailureVm?> {
  final int amount;

  ApproveFundsUsage({required this.amount});
}

class DepositFunds extends UserActionAwaitable<DepositFundsFailureVm?> {
  final int amount;

  DepositFunds({required this.amount});
}
