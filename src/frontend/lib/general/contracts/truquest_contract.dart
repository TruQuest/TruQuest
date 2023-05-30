import 'package:convert/convert.dart';
import 'package:flutter_web3/ethers.dart';

import '../../ethereum/errors/ethereum_error.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../extensions/uuid_extension.dart';

class TruQuestContract {
  static const String address = '0x32D41E4e24F97ec7D52e3c43F8DbFe209CBd0e4c';
  static const String _abi = '''[
        {
          "inputs": [
            {
              "internalType": "uint256",
              "name": "_amount",
              "type": "uint256"
            }
          ],
          "name": "deposit",
          "outputs": [],
          "stateMutability": "nonpayable",
          "type": "function"
        },
        {
          "inputs": [
            {
              "internalType": "bytes16",
              "name": "_thingId",
              "type": "bytes16"
            }
          ],
          "name": "checkThingAlreadyFunded",
          "outputs": [
            {
              "internalType": "bool",
              "name": "",
              "type": "bool"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
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
        },
        {
          "inputs": [
            {
              "internalType": "bytes16",
              "name": "_thingId",
              "type": "bytes16"
            }
          ],
          "name": "checkThingAlreadyHasSettlementProposalUnderAssessment",
          "outputs": [
            {
              "internalType": "bool",
              "name": "",
              "type": "bool"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
        {
          "inputs": [
            {
              "components": [
                {
                  "internalType": "bytes16",
                  "name": "thingId",
                  "type": "bytes16"
                },
                {
                  "internalType": "bytes16",
                  "name": "id",
                  "type": "bytes16"
                }
              ],
              "internalType": "struct TruQuest.SettlementProposalTd",
              "name": "_settlementProposal",
              "type": "tuple"
            },
            {
              "internalType": "uint8",
              "name": "_v",
              "type": "uint8"
            },
            {
              "internalType": "bytes32",
              "name": "_r",
              "type": "bytes32"
            },
            {
              "internalType": "bytes32",
              "name": "_s",
              "type": "bytes32"
            }
          ],
          "name": "fundThingSettlementProposal",
          "outputs": [],
          "stateMutability": "nonpayable",
          "type": "function"
        }
      ]''';

  final EthereumService _ethereumService;

  late final Contract? _contract;

  TruQuestContract(this._ethereumService) {
    if (_ethereumService.available) {
      _contract = Contract(address, _abi, _ethereumService.provider);
    }
  }

  Future<bool> checkThingAlreadyFunded(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return false;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return await contract.call<bool>('checkThingAlreadyFunded', [thingIdHex]);
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

      // print('Fund txn sent! Awaiting confirmations...');

      // await txnResponse.wait(2); // @@??: Do not await confirmations?

      // print('Fund txn confirmed!');

      await txnResponse.wait();
      print('Fund txn mined!');
    } catch (e) {
      print(e);
    }
  }

  Future<bool> checkThingAlreadyHasSettlementProposalUnderAssessment(
    String thingId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return false;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return await contract.call<bool>(
      'checkThingAlreadyHasSettlementProposalUnderAssessment',
      [thingIdHex],
    );
  }

  Future fundThingSettlementProposal(
    String thingId,
    String proposalId,
    String signature,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return;
    }
    if (_ethereumService.connectedAccount == null) {
      return;
    }

    var signer = _ethereumService.provider.getSigner();
    contract = contract.connect(signer);

    var thingIdHex = thingId.toSolInputFormat();
    var proposalIdHex = proposalId.toSolInputFormat();
    signature = signature.substring(2);
    var r = '0x' + signature.substring(0, 64);
    var s = '0x' + signature.substring(64, 128);
    var v = hex.decode(signature.substring(128, 130)).first;

    try {
      var txnResponse = await contract.send(
        'fundThingSettlementProposal',
        [
          [thingIdHex, proposalIdHex],
          v,
          r,
          s,
        ],
        TransactionOverride(
          gasLimit: BigInt.from(150000),
        ),
      );

      // print('Fund txn sent! Awaiting confirmations...');

      // await txnResponse.wait(2); // @@??: Do not await confirmations?

      // print('Fund txn confirmed!');
      await txnResponse.wait();
      print('Fund txn mined!');
    } catch (e) {
      print(e);
    }
  }

  Future<EthereumError?> depositFunds(int amount) async {
    var contract = _contract;
    if (contract == null) {
      return EthereumError('Metamask not installed');
    }
    if (_ethereumService.connectedAccount == null) {
      return EthereumError('No account connected');
    }

    var signer = _ethereumService.provider.getSigner();
    contract = contract.connect(signer);

    try {
      var txnResponse = await contract.send(
        'deposit',
        [BigInt.from(amount)],
        TransactionOverride(
          gasLimit: BigInt.from(150000),
        ),
      );

      await txnResponse.wait();
      print('Deposit funds txn mined!');

      return null;
    } catch (e) {
      print(e);
      return EthereumError(e.toString());
    }
  }
}
