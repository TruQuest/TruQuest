import "dart:async";

import 'user_api_service.dart';
import "../../ethereum/services/ethereum_service.dart";
import "../models/vm/user_vm.dart";

class UserService {
  final EthereumService _ethereumService;
  final UserApiService _userApiService;

  final Map<String, String> _accountToJwt = {};
  final Map<String, String> _accountToUsername = {};

  final StreamController<UserVm> _currentUserChangedEventChannel =
      StreamController<UserVm>();
  Stream<UserVm> get currentUserChanged$ =>
      _currentUserChangedEventChannel.stream;

  UserService(this._ethereumService, this._userApiService) {
    _ethereumService.connectedAccountChanged$.listen((account) {
      final UserAccountState state;
      String? username;
      if (account == null) {
        state = UserAccountState.guest;
      } else if (!_accountToJwt.containsKey(account)) {
        state = UserAccountState.connectedNotLoggedIn;
      } else {
        state = UserAccountState.connectedAndLoggedIn;
        username = _accountToUsername[account];
      }

      _currentUserChangedEventChannel.add(UserVm(
        state: state,
        ethereumAccount: account,
        username: username,
      ));
    });
  }

  UserVm getCurrentUser() {
    final UserAccountState state;
    String? username;
    if (_ethereumService.connectedAccount == null) {
      state = UserAccountState.guest;
    } else if (!_accountToJwt.containsKey(_ethereumService.connectedAccount)) {
      state = UserAccountState.connectedNotLoggedIn;
    } else {
      state = UserAccountState.connectedAndLoggedIn;
      username = _accountToUsername[_ethereumService.connectedAccount];
    }

    return UserVm(
      state: state,
      ethereumAccount: _ethereumService.connectedAccount,
      username: username,
    );
  }

  Future signUp(String username, String signature) async {
    await _userApiService.signUp(username, signature);
  }
}
