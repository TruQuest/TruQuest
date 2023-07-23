import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../../ethereum/errors/user_operation_error.dart';
import '../../general/contracts/truthserum_contract.dart';
import '../../ethereum/services/iwallet_service.dart';
import '../../ethereum/services/third_party_wallet_service.dart';
import '../../ethereum/services/user_operation_service.dart';
import '../../general/contracts/truquest_contract.dart';
import '../../widget_extensions.dart';
import 'user_api_service.dart';
import '../models/vm/user_vm.dart';
import '../../ethereum/services/local_wallet_service.dart';
import '../../general/services/local_storage.dart';
import '../../general/services/server_connector.dart';

class UserService {
  final LocalWalletService _localWalletService;
  final ThirdPartyWalletService _thirdPartyWalletService;
  final UserApiService _userApiService;
  final ServerConnector _serverConnector;
  final LocalStorage _localStorage;
  final UserOperationService _userOperationService;
  final TruQuestContract _truQuestContract;
  final TruthserumContract _truthserumContract;

  late final IWalletService _walletService;
  String? _selectedWalletName;
  String? get selectedWalletName => _selectedWalletName;

  final _currentUserChangedEventChannel = BehaviorSubject<UserVm>();
  Stream<UserVm> get currentUserChanged$ =>
      _currentUserChangedEventChannel.stream;
  UserVm? get latestCurrentUser => _currentUserChangedEventChannel.valueOrNull;

  UserService(
    this._localWalletService,
    this._thirdPartyWalletService,
    this._userApiService,
    this._serverConnector,
    this._localStorage,
    this._userOperationService,
    this._truQuestContract,
    this._truthserumContract,
  ) {
    var selectedWallet = _localStorage.getString('SelectedWallet');
    if (selectedWallet == null) {
      _reloadUser(null);
      return;
    }

    if (selectedWallet == 'Local') {
      _setupLocalWallet();
    } else {
      _setupThirdPartyWallet(selectedWallet);
    }
  }

  void _setupLocalWallet() async {
    await _localWalletService.setup();
    _walletService = _localWalletService;
    _selectedWalletName = 'Local';

    registerUserOperationBuilder(_localWalletService);

    _walletService.currentWalletAddressChanged$.listen(
      (currentWalletAddress) => _reloadUser(currentWalletAddress),
    );
  }

  Future<bool> _setupThirdPartyWallet(String walletName) async {
    bool hasConnectedAccount = await _thirdPartyWalletService.setup(
      walletName,
    );
    _walletService = _thirdPartyWalletService;
    _selectedWalletName = walletName;

    registerUserOperationBuilder(_thirdPartyWalletService);

    _walletService.currentWalletAddressChanged$.listen(
      (currentWalletAddress) => _reloadUser(currentWalletAddress),
    );

    return !hasConnectedAccount;
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

  Future createAndSaveEncryptedLocalWallet(
    String mnemonic,
    String password,
  ) async {
    await _localWalletService.createAndSaveEncryptedWallet(
      mnemonic,
      password,
    );
    await _localStorage.setString('SelectedWallet', 'Local');
    _setupLocalWallet();
  }

  Future<bool> selectThirdPartyWallet(String walletName) async {
    // @@NOTE: First save the value and THEN setup, so that if a reload
    // is needed (talking to you, Metamask), the selected wallet is picked up
    // on reload.
    await _localStorage.setString('SelectedWallet', walletName);
    bool shouldRequestAccounts = await _setupThirdPartyWallet(walletName);
    return shouldRequestAccounts;
  }

  Future signInWithEthereum() async {
    var currentWalletAddress = _walletService.currentWalletAddress!;

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

    var signature = await _walletService.personalSign(message);

    var siweResult = await _userApiService.signInWithEthereum(
      message,
      signature,
    );

    await _localStorage.setStrings(
      currentWalletAddress,
      [siweResult.token, siweResult.username],
    );

    _reloadUser(currentWalletAddress);
  }

  Future addEmail(String email) async {
    // var wallet = _localWalletService.wallet;
    // if (wallet?.locked ?? true) {
    //   print('Smart Wallet not unlocked');
    //   return;
    // }

    // await _userApiService.addEmail(email);
  }

  Future confirmEmail(String confirmationToken) async {
    // var wallet = _localWalletService.wallet;
    // if (wallet?.locked ?? true) {
    //   print('Smart Wallet not unlocked');
    //   return;
    // }

    // await _userApiService.confirmEmail(confirmationToken);
  }

  Future depositFunds(int amount) async {
    print('**************** Deposit funds ****************');

    int balance = await _truthserumContract.balanceOf(
      _walletService.currentWalletAddress!,
    );
    print('**************** Balance: $balance drops ****************');
    if (balance < amount) {
      throw UserOperationError('Not enough funds');
    }

    await await _userOperationService.sendBatch(
      actions: [
        (TruthserumContract.address, _truthserumContract.approve(amount)),
        (TruQuestContract.address, _truQuestContract.depositFunds(amount)),
      ],
    );
  }
}
