import 'dart:convert';
import 'dart:typed_data';

import 'package:convert/convert.dart';

import '../../../ethereum_js_interop.dart';

class SmartWallet {
  EOA? _owner;
  late int _currentOwnerIndex;
  late final Map<int, String> _ownerIndexToAddress;
  late final Map<int, String> _ownerIndexToWalletAddress;

  bool get locked => _owner == null;

  String get currentOwnerAddress => _ownerIndexToAddress[_currentOwnerIndex]!;
  String get currentWalletAddress =>
      _ownerIndexToWalletAddress[_currentOwnerIndex]!;

  String get _currentPrivateKey => EOA
      .fromMnemonic(_owner!.mnemonic, getAccountPath(_currentOwnerIndex))
      .privateKey;

  List<String> get walletAddresses =>
      (_ownerIndexToWalletAddress.entries.toList()
            ..sort(
              (e1, e2) => e1.key.compareTo(e2.key),
            ))
          .map((e) => e.value)
          .toList();

  SmartWallet(this._owner) {
    _ownerIndexToAddress = {};
    _ownerIndexToWalletAddress = {};
  }

  SmartWallet._(
    this._owner,
    this._currentOwnerIndex,
    this._ownerIndexToAddress,
    this._ownerIndexToWalletAddress,
  );

  void lock() => _owner = null;

  int addOwnerAccount({bool switchToAdded = true}) {
    var index =
        ((_ownerIndexToAddress.keys.toList()..sort()).lastOrNull ?? -1) + 1;
    var ownerAddress =
        EOA.fromMnemonic(_owner!.mnemonic, getAccountPath(index)).address;
    _ownerIndexToAddress[index] = ownerAddress;
    if (switchToAdded) {
      _currentOwnerIndex = index;
    }

    return index;
  }

  String getOwnerAddress(int ownerIndex) => _ownerIndexToAddress[ownerIndex]!;

  void setOwnerWalletAddress(int ownerIndex, String walletAddress) =>
      _ownerIndexToWalletAddress[ownerIndex] = walletAddress;

  void switchCurrentToWalletsOwner(String walletAddress) {
    var index = _ownerIndexToWalletAddress.entries
        .singleWhere((e) => e.value == walletAddress)
        .key;
    _currentOwnerIndex = index;
  }

  static Future<SmartWallet> fromMap(
    Map<String, dynamic> map, {
    String? password,
  }) async =>
      SmartWallet._(
        password != null
            ? await EOA.fromEncryptedJson(map['encryptedOwner'], password)
            : null,
        map['currentOwnerIndex'],
        Map.fromEntries(
          (map['ownerIndexToAddress'] as Map<String, dynamic>).entries.map(
                (entry) => MapEntry(
                  int.parse(entry.key),
                  entry.value as String,
                ),
              ),
        ),
        Map.fromEntries(
          (map['ownerIndexToWalletAddress'] as Map<String, dynamic>)
              .entries
              .map(
                (entry) => MapEntry(
                  int.parse(entry.key),
                  entry.value as String,
                ),
              ),
        ),
      );

  Future<Map<String, dynamic>> toJson({String? password}) async => {
        'encryptedOwner':
            password != null ? await _owner!.encrypt(password) : null,
        'currentOwnerIndex': _currentOwnerIndex,
        'ownerIndexToAddress': Map.fromEntries(
          _ownerIndexToAddress.entries.map(
            (entry) => MapEntry(entry.key.toString(), entry.value),
          ),
        ),
        'ownerIndexToWalletAddress': Map.fromEntries(
          _ownerIndexToWalletAddress.entries.map(
            (entry) => MapEntry(entry.key.toString(), entry.value),
          ),
        ),
      };

  String ownerSign(String message) {
    var pk = SigningKey(
      Uint8List.fromList(hex.decode(_currentPrivateKey.substring(2))),
    );
    var hash = hashMessage(Uint8List.fromList(utf8.encode(message)));

    return pk
        .signDigest(Uint8List.fromList(hex.decode(hash.substring(2))))
        .combined;
  }

  String ownerSignDigest(String digest) {
    if (digest.startsWith('0x')) digest = digest.substring(2);

    var pk = SigningKey(
      Uint8List.fromList(hex.decode(_currentPrivateKey.substring(2))),
    );
    var hash = hashMessage(Uint8List.fromList(hex.decode(digest)));

    return pk
        .signDigest(Uint8List.fromList(hex.decode(hash.substring(2))))
        .combined;
  }
}
