import 'package:flutter_web3/flutter_web3.dart';

import '../../thing/models/im/decision_im.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../extensions/uuid_extension.dart';

class AcceptancePollContract {
  static const String _address = '0x8094C98F3b0d431aDc7eaf2041fC06F8a36369e6';
  static const String _abi = '''[
    {
      "inputs": [],
      "name": "getPollDurationBlocks",
      "outputs": [
        {
          "internalType": "uint16",
          "name": "",
          "type": "uint16"
        }
      ],
      "stateMutability": "view",
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
      "name": "getPollInitBlock",
      "outputs": [
        {
          "internalType": "int256",
          "name": "",
          "type": "int256"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "_thingId",
          "type": "bytes16"
        },
        {
          "internalType": "enum AcceptancePoll.Vote",
          "name": "_vote",
          "type": "uint8"
        }
      ],
      "name": "castVote",
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
        },
        {
          "internalType": "enum AcceptancePoll.Vote",
          "name": "_vote",
          "type": "uint8"
        },
        {
          "internalType": "string",
          "name": "_reason",
          "type": "string"
        }
      ],
      "name": "castVoteWithReason",
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
        },
        {
          "internalType": "address",
          "name": "_user",
          "type": "address"
        }
      ],
      "name": "getUserIndexAmongThingVerifiers",
      "outputs": [
        {
          "internalType": "int256",
          "name": "",
          "type": "int256"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    }
  ]''';

  final EthereumService _ethereumService;

  late final Contract? _contract;

  AcceptancePollContract(this._ethereumService) {
    if (_ethereumService.available) {
      _contract = Contract(_address, _abi, _ethereumService.provider);
    }
  }

  Future<int> getPollDurationBlocks() async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    return await contract.call<int>('getPollDurationBlocks');
  }

  Future<int> getPollInitBlock(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return (await contract.call<BigInt>('getPollInitBlock', [thingIdHex]))
        .toInt();
  }

  Future castVote(String thingId, DecisionIm decision, String reason) async {
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

    try {
      TransactionResponse txnResponse;
      if (reason.isEmpty) {
        txnResponse = await contract.send(
          'castVote',
          [
            thingIdHex,
            decision.index,
          ],
        );
      } else {
        txnResponse = await contract.send(
          'castVoteWithReason',
          [
            thingIdHex,
            decision.index,
            reason,
          ],
        );
      }

      await txnResponse.wait();
      print('Cast vote txn mined!');
    } catch (e) {
      print(e);
    }
  }

  Future<int> getUserIndexAmongThingVerifiers(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return -1;
    }
    if (_ethereumService.connectedAccount == null) {
      return -1;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return (await contract.call<BigInt>(
      'getUserIndexAmongThingVerifiers',
      [
        thingIdHex,
        _ethereumService.connectedAccount,
      ],
    ))
        .toInt();
  }
}
