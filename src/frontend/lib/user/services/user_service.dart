import 'package:rxdart/rxdart.dart';

import 'user_api_service.dart';
import '../models/vm/user_vm.dart';
import '../../general/contracts/erc4337/ientrypoint_contract.dart';
import '../../ethereum/services/ethereum_api_service.dart';
import '../../ethereum/models/im/user_operation.dart';
import '../../general/contracts/dummy_contract.dart';
import '../../ethereum/models/vm/smart_wallet.dart';
import '../../ethereum/services/smart_wallet_service.dart';
import '../../general/services/local_storage.dart';
import '../../general/services/server_connector.dart';

class UserService {
  final SmartWalletService _smartWalletService;
  final UserApiService _userApiService;
  final ServerConnector _serverConnector;
  final LocalStorage _localStorage;
  final DummyContract _dummyContract;
  final EthereumApiService _ethereumApiService;
  final IEntryPointContract _entryPointContract;

  SmartWallet? _wallet;

  final _currentUserChangedEventChannel = BehaviorSubject<UserVm>();
  Stream<UserVm> get currentUserChanged$ =>
      _currentUserChangedEventChannel.stream;
  UserVm? get latestCurrentUser => _currentUserChangedEventChannel.valueOrNull;

  UserService(
    this._smartWalletService,
    this._userApiService,
    this._serverConnector,
    this._localStorage,
    this._dummyContract,
    this._ethereumApiService,
    this._entryPointContract,
  ) {
    if (_localStorage.getString('SmartWallet') == null) {
      _reloadUser(null);
      return;
    }

    _smartWalletService.getFromLocalStorage('password').then((wallet) {
      _wallet = wallet;
      _reloadUser(wallet.address);
    });
  }

  void _reloadUser(String? walletAddress) {
    bool isGuest;
    List<String>? userData;
    String? username;
    if (walletAddress == null ||
        (userData = _localStorage.getStrings(walletAddress)) == null) {
      isGuest = true;
      _serverConnector.connectToHub(username, null);
    } else {
      isGuest = false;
      username = userData!.last;
      _serverConnector.connectToHub(username, userData.first);
    }

    _currentUserChangedEventChannel.add(UserVm(
      isGuest: isGuest,
      walletAddress: walletAddress,
      username: username,
    ));
  }

  Future<SmartWallet> createSmartWallet() => _smartWalletService.createOne();

  Future encryptSmartWalletAndSaveToLocalStorage(
    SmartWallet wallet,
    String password,
  ) =>
      _smartWalletService.encryptAndSaveToLocalStorage(
        wallet,
        password,
      );

  Future signInWithEthereum(String password) async {
    _wallet = await _smartWalletService.getFromLocalStorage(password);

    var nonce = await _userApiService.getNonceForSiwe(_wallet!.address);
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
        '${_wallet!.address}\n\n'
        '$statement\n\n'
        'URI: $uri\n'
        'Version: $version\n'
        'Chain ID: $chainId\n'
        'Nonce: $nonce\n'
        'Issued At: $nowWithoutMicroseconds';

    var signature = _wallet!.ownerSign(message);

    var siweResult = await _userApiService.signInWithEthereum(
      message,
      signature,
    );

    await _localStorage.setStrings(
      _wallet!.address,
      [siweResult.token, siweResult.username],
    );

    _reloadUser(_wallet!.address);
  }

  Future addEmail(String email) async {
    if (_wallet == null) {
      print('Smart Wallet not unlocked');
      return;
    }

    await _userApiService.addEmail(email);
  }

  Future confirmEmail(String confirmationToken) async {
    if (_wallet == null) {
      print('Smart Wallet not unlocked');
      return;
    }

    await _userApiService.confirmEmail(confirmationToken);
  }

  Future foo() async {
    var userOp = await UserOperation.create()
        .from(_wallet!)
        .action(_dummyContract.setValue('asdasd;laksdl;'))
        .signed();

    print(userOp);

    var userOpHash = await _ethereumApiService.sendUserOperation(
      userOp,
      _entryPointContract.address,
    );
    print('UserOp $userOpHash Sent!');
  }
}
