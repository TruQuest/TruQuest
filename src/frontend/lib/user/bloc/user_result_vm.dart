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
