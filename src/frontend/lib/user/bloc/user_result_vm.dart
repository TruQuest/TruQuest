import "../models/vm/user_vm.dart";

abstract class UserResultVm {}

abstract class LoadCurrentUserResultVm extends UserResultVm {}

class CurrentUserLoadedVm extends LoadCurrentUserResultVm {
  final UserVm user;

  CurrentUserLoadedVm({required this.user});
}

class CurrentUserLoadingVm extends LoadCurrentUserResultVm {}

abstract class SignUpResultVm extends UserResultVm {}

class SignUpSuccessVm extends SignUpResultVm {}

class SignUpFailureVm extends SignUpResultVm {}
