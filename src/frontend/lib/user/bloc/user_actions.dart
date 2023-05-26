import 'user_result_vm.dart';
import '../../general/bloc/mixins.dart';

abstract class UserAction {}

abstract class UserActionAwaitable<T extends UserResultVm?> extends UserAction
    with AwaitableResult<T> {}

class SignInWithEthereum extends UserAction {}
