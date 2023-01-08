import 'dart:async';

import '../../general/services/server_connector.dart';
import '../../general/errors/error.dart';
import 'user_api_service.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../models/vm/user_vm.dart';

class UserService {
  final EthereumService _ethereumService;
  final UserApiService _userApiService;
  final ServerConnector _serverConnector;

  final Map<String, String> _accountToJwt = {};
  final Map<String, String> _accountToUsername = {};

  final StreamController<UserVm> _currentUserChangedEventChannel =
      StreamController<UserVm>();
  Stream<UserVm> get currentUserChanged$ =>
      _currentUserChangedEventChannel.stream;

  UserService(
    this._ethereumService,
    this._userApiService,
    this._serverConnector,
  ) {
    _ethereumService.connectedAccountChanged$.listen(
      (account) => _reloadUser(account),
    );
  }

  void _reloadUser(String? account) {
    final UserAccountState state;
    String? username;
    if (account == null) {
      state = UserAccountState.guest;
      _serverConnector.disconnectFromHub();
    } else if (!_accountToJwt.containsKey(account)) {
      state = UserAccountState.connectedNotLoggedIn;
      _serverConnector.disconnectFromHub();
    } else {
      state = UserAccountState.connectedAndLoggedIn;
      username = _accountToUsername[account];
      _serverConnector.connectToHub(_accountToJwt[account]!);
    }

    _currentUserChangedEventChannel.add(UserVm(
      state: state,
      ethereumAccount: account,
      username: username,
    ));
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

  Future<Error?> signUp(
    String account,
    String username,
    String signature,
  ) async {
    try {
      var result = await _userApiService.signUp(username, signature);
      _accountToJwt[account] = result.token;
      _accountToUsername[account] = username;
      // connectedAccount here can actually be different from account
      _reloadUser(_ethereumService.connectedAccount);
    } on Error catch (e) {
      print(e);
      return e;
    }

    return null;
  }
}
