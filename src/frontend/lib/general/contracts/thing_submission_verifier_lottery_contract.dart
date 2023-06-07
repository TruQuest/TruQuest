import 'dart:math';

import 'package:convert/convert.dart';
import 'package:flutter_web3/flutter_web3.dart';

import '../../ethereum/errors/ethereum_error.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../extensions/uuid_extension.dart';

class ThingSubmissionVerifierLotteryContract {
  static const String _address = '0x05797936947e92b35438F3fcc0562fDbDA01E6ac';
  static const String _abi = '''[
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "_thingId",
          "type": "bytes16"
        }
      ],
      "name": "getOrchestratorCommitmentDataHash",
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
        }
      ],
      "name": "getOrchestratorCommitmentUserXorDataHash",
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
          "internalType": "bytes32",
          "name": "_userData",
          "type": "bytes32"
        }
      ],
      "name": "joinLottery",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    }
  ]''';

  final EthereumService _ethereumService;

  final _random = Random.secure();

  late final Contract? _contract;

  ThingSubmissionVerifierLotteryContract(this._ethereumService) {
    if (_ethereumService.available) {
      _contract = Contract(_address, _abi, _ethereumService.provider);
    }
  }

  Future<String?> getOrchestratorCommitmentDataHash(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return await contract.call<String>(
      'getOrchestratorCommitmentDataHash',
      [thingIdHex],
    );
  }

  Future<String?> getOrchestratorCommitmentUserXorDataHash(
      String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return await contract.call<String>(
      'getOrchestratorCommitmentUserXorDataHash',
      [thingIdHex],
    );
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

  Future<EthereumError?> joinLottery(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return EthereumError('Metamask not installed');
    }
    if (_ethereumService.connectedAccount == null) {
      return EthereumError('No account connected');
    }

    var signer = _ethereumService.provider.getSigner();
    contract = contract.connect(signer);

    var thingIdHex = thingId.toSolInputFormat();
    var userData = List<int>.generate(32, (_) => _random.nextInt(256));
    var userDataHex = '0x' + hex.encode(userData);

    try {
      var txnResponse = await contract.send(
        'joinLottery',
        [thingIdHex, userDataHex],
        TransactionOverride(
          gasLimit: BigInt.from(250000),
        ),
      );

      await txnResponse.wait();
      print('Join txn mined!');

      return null;
    } catch (e) {
      print(e);
      return EthereumError(e.toString());
    }
  }
}
