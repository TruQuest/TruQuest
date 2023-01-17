import 'package:convert/convert.dart';
import 'package:flutter_web3/ethers.dart';

import '../../ethereum/services/ethereum_service.dart';
import '../extensions/uuid_extension.dart';

class TruQuestContract {
  static const String _address = '0x32D41E4e24F97ec7D52e3c43F8DbFe209CBd0e4c';
  static const String _abi = '''[
        {
          "inputs": [
            {
              "components": [
                {"internalType": "bytes16", "name": "id", "type": "bytes16"}
              ],
              "internalType": "struct TruQuest.ThingTd",
              "name": "_thing",
              "type": "tuple"
            },
            {"internalType": "uint8", "name": "_v", "type": "uint8"},
            {"internalType": "bytes32", "name": "_r", "type": "bytes32"},
            {"internalType": "bytes32", "name": "_s", "type": "bytes32"}
          ],
          "name": "fundThing",
          "outputs": [],
          "stateMutability": "nonpayable",
          "type": "function"
        }
      ]''';

  final EthereumService _ethereumService;

  late final Contract? _contract;

  TruQuestContract(this._ethereumService) {
    if (_ethereumService.available) {
      _contract = Contract(_address, _abi, _ethereumService.provider);
    }
  }

  Future fundThing(String thingId, String signature) async {
    var contract = _contract;
    if (contract == null) {
      return;
    }
    if (_ethereumService.connectedAccount == null) {
      return;
    }

    var signer =
        _ethereumService.provider.getSigner(); // @@??: What if not connected ?
    contract = contract.connect(signer);

    var thingIdHex = thingId.toSolInputFormat();
    signature = signature.substring(2);
    var r = '0x' + signature.substring(0, 64);
    var s = '0x' + signature.substring(64, 128);
    var v = hex.decode(signature.substring(128, 130)).first;

    try {
      var txnResponse = await contract.send(
        'fundThing',
        [
          [thingIdHex],
          v,
          r,
          s,
        ],
        TransactionOverride(
          gasLimit: BigInt.from(100000), // 83397
        ),
      );

      print('Txn sent! Waiting for confirmations...');

      await txnResponse.wait(1);

      print('Txn confirmed!');
    } catch (e) {
      print(e);
    }
  }
}
