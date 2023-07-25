import '../../general/bloc/actions.dart';
import 'user_result_vm.dart';
import '../../general/bloc/mixins.dart';

abstract class UserAction extends Action {
  const UserAction();
}

abstract class UserActionAwaitable<T extends UserResultVm?> extends UserAction
    with AwaitableResult<T> {}

class SelectThirdPartyWallet
    extends UserActionAwaitable<SelectThirdPartyWalletSuccessVm> {
  final String walletName;

  SelectThirdPartyWallet({required this.walletName});
}

class ConnectAccount extends UserAction {
  const ConnectAccount();
}

class GenerateMnemonic
    extends UserActionAwaitable<GenerateMnemonicSuccessVm?> {}

class CreateAndSaveEncryptedLocalWallet
    extends UserActionAwaitable<CreateAndSaveEncryptedLocalWalletSuccessVm?> {
  final String mnemonic;
  final String password;

  CreateAndSaveEncryptedLocalWallet({
    required this.mnemonic,
    required this.password,
  });
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

class DepositFunds extends UserAction {
  final int amount;

  @override
  bool get mustValidate => true;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (amount <= 0) {
      errors ??= [];
      errors.add('Amount must be bigger than 0');
    }

    return errors;
  }

  const DepositFunds({required this.amount});
}
