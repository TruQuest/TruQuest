import 'dart:math';

import 'package:convert/convert.dart';

import '../../ethereum/errors/ethereum_error.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../../ethereum_js_interop.dart';
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
      "name": "getOrchestratorCommitment",
      "outputs": [
        {
          "internalType": "int256",
          "name": "",
          "type": "int256"
        },
        {
          "internalType": "bytes32",
          "name": "",
          "type": "bytes32"
        },
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

  Future<(int, String, String)> getOrchestratorCommitment(
    String thingId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      var nullString = '0x${List.generate(64, (_) => '0').join()}';
      return (0, nullString, nullString);
    }

    var thingIdHex = thingId.toSolInputFormat();

    var result = await contract.read<List<dynamic>>(
      'getOrchestratorCommitment',
      args: [thingIdHex],
    );

    return (
      (result[0] as BigInt).toInt(),
      result[1] as String,
      result[2] as String
    );
  }

  Future<int> getLotteryDurationBlocks() async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    return await contract.read<int>('getLotteryDurationBlocks');
  }

  Future<int> getLotteryInitBlock(String thingId) async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    var thingIdHex = thingId.toSolInputFormat();

    return (await contract.read<BigInt>(
      'getLotteryInitBlock',
      args: [thingIdHex],
    ))
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

    return await contract.read<bool>(
      'checkAlreadyJoinedLottery',
      args: [
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
      var txnResponse = await contract.write(
        'joinLottery',
        args: [thingIdHex, userDataHex],
        override: TransactionOverride(
          gasLimit: 250000,
        ),
      );

      await txnResponse.wait();
      print('Join txn mined!');

      return null;
    } on ContractRequestError catch (e) {
      print('Join lottery error: [${e.code}] ${e.message}');
      return EthereumError('Error joining lottery');
    } on ContractExecError catch (e) {
      print('Join lottery error: [${e.code}] ${e.reason}');
      return EthereumError('Error joining lottery');
    }
  }
}
