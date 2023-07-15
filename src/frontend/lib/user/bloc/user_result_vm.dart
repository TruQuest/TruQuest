import '../models/vm/user_vm.dart';
import '../../ethereum/models/vm/smart_wallet.dart';

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
