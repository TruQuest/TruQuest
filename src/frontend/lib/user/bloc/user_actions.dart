import '../../ethereum_js_interop.dart';
import '../../general/bloc/actions.dart';

abstract class UserAction extends Action {
  const UserAction();
}

class GenerateConfirmationCodeAndAttestationOptions extends UserAction {
  final String email;

  const GenerateConfirmationCodeAndAttestationOptions({required this.email});
}

class SignUp extends UserAction {
  final String email;
  final String confirmationCode;
  final AttestationOptions options;

  const SignUp({
    required this.email,
    required this.confirmationCode,
    required this.options,
  });
}

class SaveKeyShareQrCodeImage extends UserAction {
  const SaveKeyShareQrCodeImage();
}

class SignInWithThirdPartyWallet extends UserAction {
  final String? walletName;

  const SignInWithThirdPartyWallet({this.walletName});
}

class SignInFromExistingDevice extends UserAction {
  const SignInFromExistingDevice();
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
