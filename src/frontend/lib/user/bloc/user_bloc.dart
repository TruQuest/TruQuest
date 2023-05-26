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
  LoadCurrentUserSuccessVm? get latestCurrentUser =>
      _currentUserChannel.valueOrNull;

  UserBloc(this._userService) {
    actionChannel.stream.listen((action) {
      if (action is SignInWithEthereum) {
        _signInWithEthereum(action);
      }
    });

    _userService.currentUserChanged$.listen((user) {
      _currentUserChannel.add(LoadCurrentUserSuccessVm(user: user));
    });
  }

  void _signInWithEthereum(SignInWithEthereum action) async {
    await _userService.signInWithEthereum();
  }
}
