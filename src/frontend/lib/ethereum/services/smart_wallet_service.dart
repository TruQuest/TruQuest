import 'dart:convert';

import '../models/vm/smart_wallet.dart';
import '../../general/contracts/erc4337/iaccount_factory_contract.dart';
import '../../ethereum_js_interop.dart';
import '../../general/services/local_storage.dart';

class SmartWalletService {
  final LocalStorage _localStorage;
  final IAccountFactoryContract accountFactoryContract;

  SmartWalletService(
    this._localStorage,
    this.accountFactoryContract,
  );

  Future<SmartWallet> createOne() async {
    var owner = EOA.createRandom();
    var address = await accountFactoryContract.getAddress(owner.address);
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
