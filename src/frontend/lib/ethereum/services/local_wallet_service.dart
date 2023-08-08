import 'dart:async';
import 'dart:convert';

import 'package:rxdart/rxdart.dart';

import '../../general/contexts/multi_stage_operation_context.dart';
import '../../user/errors/wallet_locked_error.dart';
import 'iwallet_service.dart';
import '../models/vm/local_wallet.dart';
import '../../general/contracts/erc4337/iaccount_factory_contract.dart';
import '../../ethereum_js_interop.dart';
import '../../general/services/local_storage.dart';

class LocalWalletService implements IWalletService {
  final LocalStorage _localStorage;
  final IAccountFactoryContract _accountFactoryContract;

  LocalWallet? _wallet;

  final _walletAddressesChannel = BehaviorSubject<List<String>>();
  Stream<List<String>> get walletAddresses$ => _walletAddressesChannel.stream;

  final _currentWalletAddressChangedEventChannel = BehaviorSubject<(String?, String?)>();

  @override
  Stream<(String?, String?)> get currentWalletAddressChanged$ => _currentWalletAddressChangedEventChannel.stream;

  @override
  String? get currentWalletAddress => _wallet?.currentWalletAddress;

  @override
  String? get currentOwnerAddress => _wallet?.currentOwnerAddress;

  @override
  bool get isUnlocked => !_wallet!.locked;

  LocalWalletService(
    this._localStorage,
    this._accountFactoryContract,
  );

  Future setup() async {
    var encryptedWalletJson = jsonDecode(_localStorage.getString('LocalWallet')!);
    _wallet = await LocalWallet.fromMap(encryptedWalletJson);
    _currentWalletAddressChangedEventChannel.add(
      (_wallet!.currentOwnerAddress, _wallet!.currentWalletAddress),
    );
    _walletAddressesChannel.add(_wallet!.walletAddresses);
  }

  String generateMnemonic() => Wallet.createRandom().mnemonic;

  Future createAndSaveEncryptedWallet(
    String mnemonic,
    String password,
  ) async {
    // @@TODO: Check password requirements.
    var wallet = LocalWallet(mnemonic, password);
    int index = wallet.addOwnerAccount();
    var walletAddress = await _getWalletAddress(wallet.getOwnerAddress(index));
    wallet.setOwnerWalletAddress(index, walletAddress);

    var encryptedWalletJson = await wallet.toJson(password: password);
    await _localStorage.setString(
      'LocalWallet',
      jsonEncode(encryptedWalletJson),
    );
  }

  Future<String> _getWalletAddress(String ownerAddress) => _accountFactoryContract.getAddress(ownerAddress);

  Future unlockWallet(String password) async {
    var encryptedWalletJson = jsonDecode(_localStorage.getString('LocalWallet')!);
    // @@TODO: Handle invalid password.
    _wallet = await LocalWallet.fromMap(
      encryptedWalletJson,
      password: password,
    );
  }

  Stream<Object> addAccount(MultiStageOperationContext ctx) async* {
    if (_wallet!.locked) {
      yield const WalletLockedError();

      bool unlocked = await ctx.unlockWalletTask.future;
      if (!unlocked) {
        return;
      }
    }

    int index = _wallet!.addOwnerAccount(switchToAdded: false);
    var walletAddress = await _getWalletAddress(
      _wallet!.getOwnerAddress(index),
    );
    _wallet!.setOwnerWalletAddress(index, walletAddress);

    await _updateAccountListInLocalStorage();
    _walletAddressesChannel.add(_wallet!.walletAddresses);
  }

  Future switchAccount(String walletAddress) async {
    _wallet!.switchCurrentToWalletsOwner(walletAddress);
    await _updateAccountListInLocalStorage();
    _currentWalletAddressChangedEventChannel.add(
      (_wallet!.currentOwnerAddress, _wallet!.currentWalletAddress),
    );
  }

  Future _updateAccountListInLocalStorage() async {
    var encryptedWalletJson = jsonDecode(
      _localStorage.getString('LocalWallet')!,
    );
    var walletJson = await _wallet!.toJson();

    encryptedWalletJson['currentOwnerIndex'] = walletJson['currentOwnerIndex'];
    encryptedWalletJson['ownerIndexToAddress'] = walletJson['ownerIndexToAddress'];
    encryptedWalletJson['ownerIndexToWalletAddress'] = walletJson['ownerIndexToWalletAddress'];

    await _localStorage.setString(
      'LocalWallet',
      jsonEncode(encryptedWalletJson),
    );
  }

  @override
  FutureOr<String> personalSign(String message) => _wallet!.ownerSign(message);

  @override
  FutureOr<String> personalSignDigest(String digest) => _wallet!.ownerSignDigest(digest);

  Stream<Object> revealSecretPhrase(MultiStageOperationContext ctx) async* {
    _wallet!.lock();
    yield const WalletLockedError();

    bool unlocked = await ctx.unlockWalletTask.future;
    if (!unlocked) {
      return;
    }

    yield _wallet!.mnemonic;
  }
}
