import 'user_result_vm.dart';
import '../../general/bloc/mixins.dart';

abstract class UserAction {}

abstract class UserActionAwaitable<T extends UserResultVm?> extends UserAction
    with AwaitableResult<T> {}

class SignUp extends UserActionAwaitable<SignUpFailureVm?> {
  final String account;
  final String username;
  final String signature;

  SignUp({
    required this.account,
    required this.username,
    required this.signature,
  });
}

class SignIn extends UserAction {}
