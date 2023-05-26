import '../models/vm/user_vm.dart';

abstract class UserResultVm {}

class LoadCurrentUserSuccessVm extends UserResultVm {
  final UserVm user;

  LoadCurrentUserSuccessVm({required this.user});
}
