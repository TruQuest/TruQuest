import 'dart:convert';
import 'dart:typed_data';

import 'package:convert/convert.dart';

import '../../../ethereum_js_interop.dart';

class SmartWallet {
  final EOA? owner;
  final String address;
  bool deployed;

  SmartWallet({
    required this.owner,
    required this.address,
    this.deployed = false,
  });

  static Future<SmartWallet> fromEncrypted(
    Map<String, dynamic> map, {
    String? password,
  }) async =>
      SmartWallet(
        owner: password != null
            ? await EOA.fromEncryptedJson(map['encryptedOwner'], password)
            : null,
        address: map['address'],
        deployed: map['deployed'],
      );

  void markAsDeployed() => deployed = true;

  Future<Map<String, dynamic>> toEncryptedJson(String password) async => {
        'encryptedOwner': await owner!.encrypt(password),
        'address': address,
        'deployed': deployed,
      };

  String ownerSign(String message) {
    var pk = SigningKey(
      Uint8List.fromList(hex.decode(owner!.privateKey.substring(2))),
    );
    var hash = hashMessage(Uint8List.fromList(utf8.encode(message)));
    return pk
        .signDigest(Uint8List.fromList(hex.decode(hash.substring(2))))
        .combined;
  }
}
