import 'dart:convert';

import '../models/vm/smart_wallet.dart';
import '../../ethereum_js_interop.dart';
import '../../general/services/local_storage.dart';
import '../../general/contracts/erc4337/simple_account_factory_contract.dart';

class SmartWalletService {
  final LocalStorage _localStorage;
  final SimpleAccountFactoryContract _simpleAccountFactoryContract;

  SmartWalletService(
    this._localStorage,
    this._simpleAccountFactoryContract,
  );

  Future<SmartWallet> createOne() async {
    var owner = EOA.createRandom();
    var address = await _simpleAccountFactoryContract.getAddress(owner.address);
    return SmartWallet(
      owner: owner,
      address: address,
    );
  }

  Future encryptAndSaveToLocalStorage(
    SmartWallet wallet,
    String password,
  ) async {
    // @@TODO: Check password requirements.
    var encryptedWallet = await wallet.toEncryptedJson(password);
    await _localStorage.setString('SmartWallet', jsonEncode(encryptedWallet));
  }

  Future<SmartWallet> getFromLocalStorage(String? password) {
    var encryptedWallet = jsonDecode(_localStorage.getString('SmartWallet')!);
    return SmartWallet.fromEncrypted(encryptedWallet, password: password);
  }
}
