import 'package:email_validator/email_validator.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum_js_interop.dart';
import '../../general/bloc/actions.dart';

abstract class UserAction extends Action {
  const UserAction();
}

class GenerateConfirmationCodeAndAttestationOptions extends UserAction {
  final String email;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!EmailValidator.validate(email)) {
      errors ??= [];
      errors.add('Invalid email');
    }

    return errors;
  }

  const GenerateConfirmationCodeAndAttestationOptions({required this.email});
}

class SignUp extends UserAction {
  final String email;
  final String confirmationCode;
  final AttestationOptions options;

  @override
  List<String>? validate() {
    List<String>? errors;
    if (!EmailValidator.validate(email)) {
      errors ??= [];
      errors.add('Invalid email');
    }
    if (dotenv.env['ENVIRONMENT'] != 'Development' &&
        (confirmationCode.length != 6 || int.tryParse(confirmationCode) == null)) {
      errors ??= [];
      errors.add('Confirmation code must be 6 digits long');
    }

    return errors;
  }

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
      errors.add('Invalid amount');
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
      errors.add('Invalid amount');
    }

    return errors;
  }

  const WithdrawFunds({required this.amount});
}
