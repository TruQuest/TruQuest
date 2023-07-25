import 'user_result_vm.dart';
import '../../general/bloc/mixins.dart';

abstract class UserAction {
  const UserAction();
}

abstract class UserActionAwaitable<T extends UserResultVm?> extends UserAction
    with AwaitableResult<T> {}

class SelectThirdPartyWallet
    extends UserActionAwaitable<SelectThirdPartyWalletSuccessVm> {
  final String walletName;

  SelectThirdPartyWallet({required this.walletName});
}

class ConnectAccount extends UserActionAwaitable<ConnectAccountSuccessVm?> {}

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
