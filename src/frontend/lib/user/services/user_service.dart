import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../../general/services/local_storage.dart';
import '../../general/services/server_connector.dart';
import 'user_api_service.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../models/vm/user_vm.dart';

class UserService {
  final EthereumService _ethereumService;
  final UserApiService _userApiService;
  final ServerConnector _serverConnector;
  final LocalStorage _localStorage;

  final BehaviorSubject<UserVm> _currentUserChangedEventChannel =
      BehaviorSubject<UserVm>();
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
      _serverConnector.connectToHub(username, null);
    } else if ((userData = _localStorage.getStrings(account)) == null) {
      state = UserAccountState.connectedNotLoggedIn;
      _serverConnector.connectToHub(username, null);
    } else {
      state = UserAccountState.connectedAndLoggedIn;
      username = userData!.last;
      _serverConnector.connectToHub(username, userData.first);
    }

    _currentUserChangedEventChannel.add(UserVm(
      state: state,
      ethereumAccount: account,
      username: username,
    ));
  }

  Future signInWithEthereum() async {
    var account = _ethereumService.connectedAccount;
    if (account == null) {
      return;
    }

    var nonce = await _userApiService.getNonceForSiwe(account);
    var result = await _ethereumService.signSiweMessage(account, nonce);
    if (result.isLeft) {
      print(result.left);
      return;
    }

    var message = result.right.item1;
    var signature = result.right.item2;

    var siweResult = await _userApiService.signInWithEthereum(
      message,
      signature,
    );

    await _localStorage.setStrings(
      account,
      [siweResult.token, siweResult.username],
    );

    _reloadUser(_ethereumService.connectedAccount);
  }
}
