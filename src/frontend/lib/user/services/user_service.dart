import 'package:rxdart/rxdart.dart';

import '../../ethereum/services/user_operation_service.dart';
import '../../general/contracts/truquest_contract.dart';
import '../errors/wallet_locked_error.dart';
import 'user_api_service.dart';
import '../models/vm/user_vm.dart';
import '../../ethereum/models/vm/smart_wallet.dart';
import '../../ethereum/services/smart_wallet_service.dart';
import '../../general/services/local_storage.dart';
import '../../general/services/server_connector.dart';
import '../../general/errors/error.dart';

class UserService {
  final SmartWalletService _smartWalletService;
  final UserApiService _userApiService;
  final ServerConnector _serverConnector;
  final LocalStorage _localStorage;
  final UserOperationService _userOperationService;
  final TruQuestContract _truQuestContract;

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
    this._userOperationService,
    this._truQuestContract,
  ) {
    if (_localStorage.getString('SmartWallet') == null) {
      _reloadUser(null);
      return;
    }

    _smartWalletService.getFromLocalStorage(null).then((wallet) {
      _walletAddressesChannel.add(wallet.walletAddresses);
      _reloadUser(wallet.currentWalletAddress);
    });
  }

  void _reloadUser(String? currentWalletAddress) {
    String? userId;
    String? username;
    List<String>? userData;
    if (currentWalletAddress == null ||
        (userData = _localStorage.getStrings(currentWalletAddress)) == null) {
      _serverConnector.connectToHub(username, null);
    } else {
      userId = currentWalletAddress.substring(2).toLowerCase();
      username = userData!.last;
      _serverConnector.connectToHub(username, userData.first);
    }

    _currentUserChangedEventChannel.add(UserVm(
      id: userId,
      username: username,
      walletAddress: currentWalletAddress,
    ));
  }

  String generateMnemonic() => _smartWalletService.generateMnemonic();

  Future createAndSaveEncryptedSmartWallet(
    String mnemonic,
    String password,
  ) =>
      _smartWalletService.createAndSaveEncryptedSmartWallet(
        mnemonic,
        password,
      );

  Future signInWithEthereum(String password) async {
    // @@NOTE: Always get the wallet from the encrypted, even if we
    // have previously unlocked it.
    // @@TODO: Handle invalid password.
    var wallet = await _smartWalletService.getFromLocalStorage(password);
    var currentWalletAddress = wallet.currentWalletAddress;

    var nonce = await _userApiService.getNonceForSiwe(currentWalletAddress);
    var domain = 'truquest.io';
    var statement =
        'I accept the TruQuest Terms of Service: https://truquest.io/tos';
    var uri = 'https://truquest.io/';
    var version = 1;
    var chainId = 31337;

    var now = DateTime.now().toUtc().toIso8601String();
    int indexOfDot = now.indexOf('.');
    var nowWithoutMicroseconds = now.substring(0, indexOfDot + 4) + 'Z';
    var message =
        '$domain wants you to sign in with your Ethereum ERC-4337 account:\n'
        '$currentWalletAddress\n\n'
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
      currentWalletAddress,
      [siweResult.token, siweResult.username],
    );

    _walletAddressesChannel.add(wallet.walletAddresses);

    _reloadUser(currentWalletAddress);
  }

  Future addEmail(String email) async {
    var wallet = _smartWalletService.wallet;
    if (wallet?.locked ?? true) {
      print('Smart Wallet not unlocked');
      return;
    }

    await _userApiService.addEmail(email);
  }

  Future confirmEmail(String confirmationToken) async {
    var wallet = _smartWalletService.wallet;
    if (wallet?.locked ?? true) {
      print('Smart Wallet not unlocked');
      return;
    }

    await _userApiService.confirmEmail(confirmationToken);
  }

  Future unlockWallet(String password) async {
    // @@TODO: Handle invalid password.
    await _smartWalletService.getFromLocalStorage(password);
  }

  Future<WalletLockedError?> addAccount() async {
    var wallet = _smartWalletService.wallet!;
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
    var wallet = _smartWalletService.wallet!;
    wallet.switchCurrentToWalletsOwner(walletAddress);
    await _smartWalletService.updateAccountListInLocalStorage(wallet);
    _reloadUser(wallet.currentWalletAddress);
  }

  Future<Error?> depositFunds(int amount) async {
    var wallet = _smartWalletService.wallet!;
    if (wallet.locked) {
      return WalletLockedError();
    }

    print('**************** Deposit funds ****************');

    return await _userOperationService.send(
      from: wallet,
      target: TruQuestContract.address,
      action: _truQuestContract.depositFunds(amount),
    );
  }
}
