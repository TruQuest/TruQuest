import 'dart:convert';

import '../models/vm/smart_wallet.dart';
import '../../general/contracts/erc4337/iaccount_factory_contract.dart';
import '../../ethereum_js_interop.dart';
import '../../general/services/local_storage.dart';

class SmartWalletService {
  final LocalStorage _localStorage;
  final IAccountFactoryContract _accountFactoryContract;

  SmartWallet? _wallet;
  SmartWallet? get wallet => _wallet;

  SmartWalletService(
    this._localStorage,
    this._accountFactoryContract,
  );

  String generateMnemonic() => Wallet.createRandom().mnemonic;

  Future createAndSaveEncryptedSmartWallet(
    String mnemonic,
    String password,
  ) async {
    // @@TODO: Check password requirements.
    var wallet = SmartWallet(mnemonic, password);
    int index = wallet.addOwnerAccount();
    var walletAddress = await _accountFactoryContract.getAddress(
      wallet.getOwnerAddress(index),
    );
    wallet.setOwnerWalletAddress(index, walletAddress);

    var encryptedWalletJson = await wallet.toJson(password: password);
    await _localStorage.setString(
      'SmartWallet',
      jsonEncode(encryptedWalletJson),
    );
    _wallet = wallet..lock();
  }

  Future<SmartWallet> getFromLocalStorage(String? password) async {
    var encryptedWalletJson = jsonDecode(
      _localStorage.getString('SmartWallet')!,
    );
    _wallet = await SmartWallet.fromMap(
      encryptedWalletJson,
      password: password,
    );

    return _wallet!;
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
