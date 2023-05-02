import 'dart:math';

import 'package:convert/convert.dart';
import 'package:flutter_web3/flutter_web3.dart';

import '../../ethereum/services/ethereum_service.dart';
import '../extensions/uuid_extension.dart';

class ThingSubmissionVerifierLotteryContract {
  static const String _address = '0x05797936947e92b35438F3fcc0562fDbDA01E6ac';
  static const String _abi = '''[
    {
      "inputs": [],
      "name": "getLotteryDurationBlocks",
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
      "name": "getLotteryInitBlock",
      "outputs": [
        {
          "internalType": "int64",
          "name": "",
          "type": "int64"
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
          "internalType": "address",
          "name": "_user",
          "type": "address"
        }
      ],
      "name": "checkAlreadyJoinedLottery",
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
      "name": "checkAlreadyPreJoinedLottery",
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
          "internalType": "bytes32",
          "name": "_data",
          "type": "bytes32"
        }
      ],
      "name": "computeHash",
      "outputs": [
        {
          "internalType": "bytes32",
          "name": "",
          "type": "bytes32"
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
          "internalType": "bytes32",
          "name": "_data",
          "type": "bytes32"
        }
      ],
      "name": "joinLottery",
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
          "internalType": "bytes32",
          "name": "_dataHash",
          "type": "bytes32"
        }
      ],
      "name": "preJoinLottery",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    }
  ]''';

  final EthereumService _ethereumService;

  final _random = Random.secure();

  late final Contract? _contract;

  final Map<String, String> _commitmentIdToData = {};

  ThingSubmissionVerifierLotteryContract(this._ethereumService) {
    if (_ethereumService.available) {
      _contract = Contract(_address, _abi, _ethereumService.provider);
    }
  }

  Future<int> getLotteryDurationBlocks() async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    return await contract.call<int>('getLotteryDurationBlocks');
  }

  Future<int> getLotteryInitBlock(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return (await contract.call<BigInt>('getLotteryInitBlock', [thingIdHex]))
        .toInt();
  }

  Future<bool?> checkAlreadyPreJoinedLottery(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }
    if (_ethereumService.connectedAccount == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return await contract.call<bool>(
      'checkAlreadyPreJoinedLottery',
      [
        thingIdHex,
        _ethereumService.connectedAccount,
      ],
    );
  }

  Future<bool?> checkAlreadyJoinedLottery(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }
    if (_ethereumService.connectedAccount == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return await contract.call<bool>(
      'checkAlreadyJoinedLottery',
      [
        thingIdHex,
        _ethereumService.connectedAccount,
      ],
    );
  }

  Future preJoinLottery(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return;
    }
    if (_ethereumService.connectedAccount == null) {
      return;
    }

    var signer = _ethereumService.provider.getSigner();
    var address = await signer.getAddress();
    contract = contract.connect(signer);

    var data = List<int>.generate(32, (_) => _random.nextInt(256));
    var dataHex = '0x' + hex.encode(data);
    print('dataHex: $dataHex');
    var dataHashHex = await contract.call<String>('computeHash', [dataHex]);
    print('dataHashHex: $dataHashHex');

    var thingIdHex = thingId.toSolInputFormat();

    try {
      var txnResponse = await contract.send(
        'preJoinLottery',
        [thingIdHex, dataHashHex],
        TransactionOverride(
          gasLimit: BigInt.from(250000),
        ),
      );

      print('PreJoined lottery! Awaiting confirmations...');

      // @@??: Why is it enough to mine just 1 block for this to complete?
      await txnResponse.wait(2);
      // @@NOTE: Because we await confirmation, the lottery info does not get updated right away, which leads to
      // Commit to Lottery button being active for longer than it should be.

      print('PreJoin txn confirmed!');

      _commitmentIdToData['$thingId|$address'] = dataHex;
    } catch (e) {
      print(e);
    }
  }

  Future joinLottery(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return;
    }
    if (_ethereumService.connectedAccount == null) {
      return;
    }

    var signer = _ethereumService.provider.getSigner();
    var address = await signer.getAddress();
    contract = contract.connect(signer);

    if (!_commitmentIdToData.containsKey('$thingId|$address')) {
      return;
    }

    var thingIdHex = thingId.toSolInputFormat();
    var dataHex = _commitmentIdToData['$thingId|$address'];

    try {
      var txnResponse = await contract.send(
        'joinLottery',
        [thingIdHex, dataHex],
        TransactionOverride(
          gasLimit: BigInt.from(250000),
        ),
      );

      print('Joined lottery! Awaiting confirmations...');

      await txnResponse.wait(2);

      print('Join txn confirmed!');

      _commitmentIdToData.remove('$thingId|$address');
    } catch (e) {
      print(e);
    }
  }
}
