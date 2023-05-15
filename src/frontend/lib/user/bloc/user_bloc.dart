import 'dart:async';

import 'package:rxdart/rxdart.dart';

import 'user_result_vm.dart';
import '../services/user_service.dart';
import 'user_actions.dart';
import '../../general/bloc/bloc.dart';

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;

  final BehaviorSubject<LoadCurrentUserSuccessVm> _currentUserChannel =
      BehaviorSubject<LoadCurrentUserSuccessVm>();
  Stream<LoadCurrentUserSuccessVm> get currentUser$ =>
      _currentUserChannel.stream;

  UserBloc(this._userService) {
    actionChannel.stream.listen((action) {
      if (action is SignUp) {
        _signUp(action);
      } else if (action is SignIn) {
        _signIn(action);
      }
    });

    _userService.currentUserChanged$.listen((user) {
      _currentUserChannel.add(LoadCurrentUserSuccessVm(user: user));
    });
  }

  void _signUp(SignUp action) async {
    var error = await _userService.signUp(
      action.account,
      action.username,
      action.signature,
    );
    action.complete(error != null ? SignUpFailureVm() : null);
  }

  void _signIn(SignIn action) async {
    await _userService.signIn();
  }
}
