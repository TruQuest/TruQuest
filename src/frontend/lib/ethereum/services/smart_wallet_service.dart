import 'dart:convert';

import '../models/vm/smart_wallet.dart';
import '../../general/contracts/erc4337/iaccount_factory_contract.dart';
import '../../ethereum_js_interop.dart';
import '../../general/services/local_storage.dart';

class SmartWalletService {
  final LocalStorage _localStorage;
  final IAccountFactoryContract _accountFactoryContract;

  SmartWalletService(
    this._localStorage,
    this._accountFactoryContract,
  );

  Future<SmartWallet> createOne() async {
    var owner = EOA.createRandom();
    var wallet = SmartWallet(owner);
    int index = wallet.addOwnerAccount();
    var walletAddress = await _accountFactoryContract.getAddress(
      wallet.getOwnerAddress(index),
    );
    wallet.setOwnerWalletAddress(index, walletAddress);

    return wallet;
  }

  Future<SmartWallet> createOneFromMnemonic(String mnemonic) async {
    var owner = EOA.fromMnemonic(mnemonic);
    var wallet = SmartWallet(owner);
    int index = wallet.addOwnerAccount();
    var walletAddress = await _accountFactoryContract.getAddress(
      wallet.getOwnerAddress(index),
    );
    wallet.setOwnerWalletAddress(index, walletAddress);

    return wallet;
  }

  Future encryptAndSaveToLocalStorage(
    SmartWallet wallet,
    String password,
  ) async {
    // @@TODO: Check password requirements.
    var encryptedWalletJson = await wallet.toJson(password: password);
    await _localStorage.setString(
      'SmartWallet',
      jsonEncode(encryptedWalletJson),
    );
  }

  Future<SmartWallet> getFromLocalStorage(String? password) {
    var encryptedWalletJson = jsonDecode(
      _localStorage.getString('SmartWallet')!,
    );
    return SmartWallet.fromMap(encryptedWalletJson, password: password);
  }

  Future<String> getWalletAddress(String ownerAddress) =>
      _accountFactoryContract.getAddress(ownerAddress);

  Future updateAccountListInLocalStorage(SmartWallet wallet) async {
    var encryptedWalletJson = jsonDecode(
      _localStorage.getString('SmartWallet')!,
    );
    var walletJson = await wallet.toJson();

    encryptedWalletJson['currentOwnerIndex'] = walletJson['currentOwnerIndex'];
    encryptedWalletJson['ownerIndexToAddress'] =
        walletJson['ownerIndexToAddress'];
    encryptedWalletJson['ownerIndexToWalletAddress'] =
        walletJson['ownerIndexToWalletAddress'];

    await _localStorage.setString(
      'SmartWallet',
      jsonEncode(encryptedWalletJson),
    );
  }
}
