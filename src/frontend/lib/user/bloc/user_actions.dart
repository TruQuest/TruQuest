import '../../general/bloc/actions.dart';

abstract class UserAction extends Action {
  const UserAction();
}

class SelectThirdPartyWallet extends UserAction {
  final String walletName;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!(walletName == 'Metamask' || walletName == 'CoinbaseWallet' || walletName == 'WalletConnect')) {
      errors ??= [];
      errors.add('Unsupported wallet');
    }

    return errors;
  }

  const SelectThirdPartyWallet({required this.walletName});
}

class ConnectAccount extends UserAction {
  const ConnectAccount();
}

class GenerateMnemonic extends UserAction {
  const GenerateMnemonic();
}

class CreateAndSaveEncryptedLocalWallet extends UserAction {
  final String mnemonic;
  final String password;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (mnemonic.split(' ').length != 12) {
      errors ??= [];
      errors.add('Mnemonic must be exactly 12 words long');
    }
    if (password.length < 8) {
      errors ??= [];
      errors.add('Password must be at least 8 characters long');
    }

    return errors;
  }

  const CreateAndSaveEncryptedLocalWallet({
    required this.mnemonic,
    required this.password,
  });
}

class SignInWithEthereum extends UserAction {
  const SignInWithEthereum();
}

class AddEmail extends UserAction {
  final String email;

  const AddEmail({required this.email});
}

class ConfirmEmail extends UserAction {
  final String confirmationToken;

  const ConfirmEmail({required this.confirmationToken});
}

class UnlockWallet extends UserAction {
  final String password;

  const UnlockWallet({required this.password});
}

class AddAccount extends UserAction {
  const AddAccount();
}

class SwitchAccount extends UserAction {
  final String walletAddress;

  @override
  List<String>? validate() {
    List<String>? errors;
    // @@TODO
    return errors;
  }

  const SwitchAccount({required this.walletAddress});
}

class DepositFunds extends UserAction {
  final int amount;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (amount <= 0) {
      errors ??= [];
      errors.add('Amount must be greater than 0');
    }

    return errors;
  }

  const DepositFunds({required this.amount});
}

class WithdrawFunds extends UserAction {
  final int amount;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (amount <= 0) {
      errors ??= [];
      errors.add('Amount must be greater than 0');
    }

    return errors;
  }

  const WithdrawFunds({required this.amount});
}

class RevealSecretPhrase extends UserAction {
  const RevealSecretPhrase();
}

class SignUp extends UserAction {
  final String email;

  const SignUp({required this.email});
}

class FinishSignUp extends UserAction {
  final String email;
  final String confirmationCode;

  const FinishSignUp({
    required this.email,
    required this.confirmationCode,
  });
}
