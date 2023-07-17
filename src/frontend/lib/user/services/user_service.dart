import 'package:rxdart/rxdart.dart';

import '../errors/wallet_locked_error.dart';
import 'user_api_service.dart';
import '../models/vm/user_vm.dart';
import '../../ethereum/models/vm/smart_wallet.dart';
import '../../ethereum/services/smart_wallet_service.dart';
import '../../general/services/local_storage.dart';
import '../../general/services/server_connector.dart';

class UserService {
  final SmartWalletService _smartWalletService;
  final UserApiService _userApiService;
  final ServerConnector _serverConnector;
  final LocalStorage _localStorage;

  SmartWallet? _wallet;
  SmartWallet? get wallet => _wallet;

  final _currentUserChangedEventChannel = BehaviorSubject<UserVm>();
  Stream<UserVm> get currentUserChanged$ =>
      _currentUserChangedEventChannel.stream;
  UserVm? get latestCurrentUser => _currentUserChangedEventChannel.valueOrNull;

  final _walletAddressesChannel = BehaviorSubject<List<String>>();
  Stream<List<String>> get walletAddresses$ => _walletAddressesChannel.stream;

  UserService(
    this._smartWalletService,
    this._userApiService,
    this._serverConnector,
    this._localStorage,
  ) {
    if (_localStorage.getString('SmartWallet') == null) {
      _reloadUser(null);
      return;
    }

    _smartWalletService.getFromLocalStorage(null).then((wallet) {
      _reloadUser(wallet);
    });
  }

  void _reloadUser(SmartWallet? wallet) {
    _wallet = wallet;
    String? userId;
    String? username;
    List<String>? userData;
    if (wallet == null ||
        (userData = _localStorage.getStrings(wallet.currentWalletAddress)) ==
            null) {
      _serverConnector.connectToHub(username, null);
    } else {
      userId = wallet.currentWalletAddress.substring(2).toLowerCase();
      username = userData!.last;
      _serverConnector.connectToHub(username, userData.first);
    }

    _currentUserChangedEventChannel.add(UserVm(
      id: userId,
      username: username,
      walletAddress: wallet?.currentWalletAddress,
    ));
  }

  Future<SmartWallet> createSmartWallet() => _smartWalletService.createOne();

  Future<SmartWallet> createSmartWalletFromMnemonic(String mnemonic) =>
      _smartWalletService.createOneFromMnemonic(mnemonic);

  Future encryptSmartWalletAndSaveToLocalStorage(
    SmartWallet wallet,
    String password,
  ) =>
      _smartWalletService.encryptAndSaveToLocalStorage(
        wallet,
        password,
      );

  Future signInWithEthereum(String password) async {
    // @@NOTE: Always get the wallet from the encrypted, even if we
    // have previously unlocked it.
    // @@TODO: Handle invalid password.
    var wallet = await _smartWalletService.getFromLocalStorage(password);

    var nonce = await _userApiService.getNonceForSiwe(
      wallet.currentWalletAddress,
    );
    var domain = 'truquest.io';
    var statement =
        'I accept the TruQuest Terms of Service: https://truquest.io/tos';
    var uri = 'https://truquest.io/';
    var version = 1;
    var chainId = 1337;

    var now = DateTime.now().toUtc().toIso8601String();
    int indexOfDot = now.indexOf('.');
    var nowWithoutMicroseconds = now.substring(0, indexOfDot + 4) + 'Z';
    var message =
        '$domain wants you to sign in with your Ethereum ERC-4337 account:\n'
        '${wallet.currentWalletAddress}\n\n'
        '$statement\n\n'
        'URI: $uri\n'
        'Version: $version\n'
        'Chain ID: $chainId\n'
        'Nonce: $nonce\n'
        'Issued At: $nowWithoutMicroseconds';

    var signature = wallet.ownerSign(message);

    var siweResult = await _userApiService.signInWithEthereum(
      message,
      signature,
    );

    await _localStorage.setStrings(
      wallet.currentWalletAddress,
      [siweResult.token, siweResult.username],
    );

    _walletAddressesChannel.add(wallet.walletAddresses);

    _reloadUser(wallet);
  }

  Future addEmail(String email) async {
    if (_wallet?.locked ?? true) {
      print('Smart Wallet not unlocked');
      return;
    }

    await _userApiService.addEmail(email);
  }

  Future confirmEmail(String confirmationToken) async {
    if (_wallet?.locked ?? true) {
      print('Smart Wallet not unlocked');
      return;
    }

    await _userApiService.confirmEmail(confirmationToken);
  }

  Future unlockWallet(String password) async {
    // @@TODO: Handle invalid password.
    var wallet = await _smartWalletService.getFromLocalStorage(password);
    _reloadUser(wallet);
  }

  Future<WalletLockedError?> addAccount() async {
    var wallet = _wallet!;
    if (wallet.locked) {
      return WalletLockedError();
    }

    int index = wallet.addOwnerAccount(switchToAdded: false);
    var walletAddress = await _smartWalletService.getWalletAddress(
      wallet.getOwnerAddress(index),
    );
    wallet.setOwnerWalletAddress(index, walletAddress);

    await _smartWalletService.updateAccountListInLocalStorage(wallet);

    _walletAddressesChannel.add(wallet.walletAddresses);

    return null;
  }

  Future switchAccount(String walletAddress) async {
    var wallet = _wallet!;
    wallet.switchCurrentToWalletsOwner(walletAddress);
    await _smartWalletService.updateAccountListInLocalStorage(wallet);
    _reloadUser(wallet);
  }
}
