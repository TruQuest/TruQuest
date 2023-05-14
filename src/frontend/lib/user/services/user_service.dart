import 'dart:async';

import '../../general/services/local_storage.dart';
import '../../general/services/server_connector.dart';
import '../../general/errors/error.dart';
import 'user_api_service.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../models/vm/user_vm.dart';

class UserService {
  final EthereumService _ethereumService;
  final UserApiService _userApiService;
  final ServerConnector _serverConnector;
  final LocalStorage _localStorage;

  final StreamController<UserVm> _currentUserChangedEventChannel =
      StreamController<UserVm>();
  Stream<UserVm> get currentUserChanged$ =>
      _currentUserChangedEventChannel.stream;

  UserService(
    this._ethereumService,
    this._userApiService,
    this._serverConnector,
    this._localStorage,
  ) {
    _ethereumService.connectedAccountChanged$.listen(
      (account) => _reloadUser(account),
    );
  }

  void _reloadUser(String? account) {
    final UserAccountState state;
    List<String>? userData;
    String? username;
    if (account == null) {
      state = UserAccountState.guest;
      _serverConnector.connectToHub(null);
    } else if ((userData = _localStorage.getStrings(account)) == null) {
      state = UserAccountState.connectedNotLoggedIn;
      _serverConnector.connectToHub(null);
    } else {
      state = UserAccountState.connectedAndLoggedIn;
      username = userData!.last;
      _serverConnector.connectToHub(userData.first);
    }

    _currentUserChangedEventChannel.add(UserVm(
      state: state,
      ethereumAccount: account,
      username: username,
    ));
  }

  Future<Error?> signUp(
    String account,
    String username,
    String signature,
  ) async {
    try {
      var result = await _userApiService.signUp(username, signature);
      await _localStorage.setStrings(account, [result.token, username]);
      // connectedAccount here can actually be different from account
      _reloadUser(_ethereumService.connectedAccount);
    } on Error catch (e) {
      print(e);
      return e;
    }

    return null;
  }

  Future signIn() async {
    var data = await _userApiService.getSignInData();
    var result = await _ethereumService.signSignInMessage(
      data.timestamp,
      data.signature,
    );
    if (result.isLeft) {
      print(result.left);
      return;
    }

    var account = result.right.item1;
    var signature = result.right.item2;

    var signInResult = await _userApiService.signIn(
      data.timestamp,
      data.signature,
      signature,
    );

    await _localStorage.setStrings(
      account,
      [signInResult.token, signInResult.username],
    );
    // connectedAccount here can actually be different from account
    _reloadUser(_ethereumService.connectedAccount);
  }
}
