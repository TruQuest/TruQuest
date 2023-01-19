import '../models/vm/user_vm.dart';

abstract class UserResultVm {}

// @@TODO: Rename this.
class LoadCurrentUserSuccessVm extends UserResultVm {
  final UserVm user;

  LoadCurrentUserSuccessVm({required this.user});
}

class SignUpFailureVm extends UserResultVm {}
