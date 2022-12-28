import "dart:async";

import "user_result_vm.dart";
import "../services/user_service.dart";
import "user_actions.dart";
import "../../general/bloc/bloc.dart";

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;

  final StreamController<LoadCurrentUserResultVm> _currentUserChannel =
      StreamController<LoadCurrentUserResultVm>.broadcast();
  Stream<LoadCurrentUserResultVm> get currentUser$ =>
      _currentUserChannel.stream;

  UserBloc(this._userService) {
    actionChannel.stream.listen((action) {
      if (action is LoadCurrentUser) {
        _loadCurrentUser(action);
      } else if (action is SignUp) {
        _signUp(action);
      }
    });

    _userService.currentUserChanged$.listen((user) {
      _currentUserChannel.add(CurrentUserLoadedVm(user: user));
    });
  }

  @override
  void dispose({UserAction? cleanupAction}) {}

  void _loadCurrentUser(LoadCurrentUser action) async {
    var user = _userService.getCurrentUser();
    _currentUserChannel.add(CurrentUserLoadedVm(user: user));
  }

  void _signUp(SignUp action) async {
    await _userService.signUp(action.username, action.signature);
  }
}
