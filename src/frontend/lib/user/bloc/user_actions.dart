import 'user_result_vm.dart';
import '../../ethereum/models/vm/smart_wallet.dart';
import '../../general/bloc/mixins.dart';

abstract class UserAction {
  const UserAction();
}

abstract class UserActionAwaitable<T extends UserResultVm?> extends UserAction
    with AwaitableResult<T> {}

class CreateSmartWallet
    extends UserActionAwaitable<CreateSmartWalletSuccessVm?> {}

class EncryptAndSaveSmartWallet
    extends UserActionAwaitable<EncryptAndSaveSmartWalletSuccessVm?> {
  final SmartWallet wallet;
  final String password;

  EncryptAndSaveSmartWallet({
    required this.wallet,
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
