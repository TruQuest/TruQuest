import 'dart:convert';
import 'dart:typed_data';

import 'package:convert/convert.dart';

import '../../../ethereum_js_interop.dart';

class SmartWallet {
  final EOA? owner;
  final String address;

  SmartWallet({
    required this.owner,
    required this.address,
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
      );

  Future<Map<String, dynamic>> toEncryptedJson(String password) async => {
        'encryptedOwner': await owner!.encrypt(password),
        'address': address,
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

  String ownerSignDigest(String digest) {
    if (digest.startsWith('0x')) digest = digest.substring(2);

    var pk = SigningKey(
      Uint8List.fromList(hex.decode(owner!.privateKey.substring(2))),
    );
    var hash = hashMessage(Uint8List.fromList(hex.decode(digest)));

    return pk
        .signDigest(Uint8List.fromList(hex.decode(hash.substring(2))))
        .combined;
  }
}
