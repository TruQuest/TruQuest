import "dart:async";

import 'package:rxdart/rxdart.dart';

import "user_result_vm.dart";
import "../services/user_service.dart";
import "user_actions.dart";
import "../../general/bloc/bloc.dart";

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;

  final BehaviorSubject<LoadCurrentUserSuccessVm> _currentUserChannel =
      BehaviorSubject<LoadCurrentUserSuccessVm>();
  Stream<LoadCurrentUserSuccessVm> get currentUser$ =>
      _currentUserChannel.stream;
  LoadCurrentUserSuccessVm get currentUser$last => _currentUserChannel.value;

  UserBloc(this._userService) {
    actionChannel.stream.listen((action) {
      if (action is LoadCurrentUser) {
        _loadCurrentUser(action);
      } else if (action is SignUp) {
        _signUp(action);
      }
    });

    _userService.currentUserChanged$.listen((user) {
      _currentUserChannel.add(LoadCurrentUserSuccessVm(user: user));
    });
  }

  @override
  void dispose({UserAction? cleanupAction}) {}

  void _loadCurrentUser(LoadCurrentUser action) async {
    var user = _userService.getCurrentUser();
    _currentUserChannel.add(LoadCurrentUserSuccessVm(user: user));
  }

  void _signUp(SignUp action) async {
    var error = await _userService.signUp(
      action.account,
      action.username,
      action.signature,
    );
    action.complete(error != null ? SignUpFailureVm() : null);
  }
}
