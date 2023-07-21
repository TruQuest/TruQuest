import 'dart:math';

import 'package:convert/convert.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';
import '../extensions/uuid_extension.dart';

class ThingSubmissionVerifierLotteryContract {
  static const String address = '0xC1C9E1d2aC4CBf0c7d8DcFa1d500D472a61f02f3';
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

  final _random = Random.secure();

  late final Abi _interface;
  late final Contract _contract;

  ThingSubmissionVerifierLotteryContract(
    EthereumRpcProvider ethereumRpcProvider,
  ) {
    _interface = Abi(_abi);
    _contract = Contract(
      address,
      _abi,
      ethereumRpcProvider.provider,
    );
  }

  Future<(int, String, String)> getOrchestratorCommitment(
    String thingId,
  ) async {
    var result = await _contract.read<List<dynamic>>(
      'getOrchestratorCommitment',
      args: [thingId.toSolInputFormat()],
    );

    return (
      (result[0] as BigInt).toInt(),
      result[1] as String,
      result[2] as String
    );
  }

  Future<int> getLotteryDurationBlocks() =>
      _contract.read<int>('getLotteryDurationBlocks');

  Future<int?> getLotteryInitBlock(String thingId) async {
    var initBlock = await _contract.read<BigInt>(
      'getLotteryInitBlock',
      args: [thingId.toSolInputFormat()],
    );
    return initBlock != BigInt.zero ? initBlock.toInt() : null;
  }

  Future<bool> checkAlreadyJoinedLottery(
    String thingId,
    String walletAddress,
  ) =>
      _contract.read<bool>(
        'checkAlreadyJoinedLottery',
        args: [thingId.toSolInputFormat(), walletAddress],
      );

  String joinLottery(String thingId) {
    var thingIdHex = thingId.toSolInputFormat();
    var userData = List<int>.generate(32, (_) => _random.nextInt(256));
    var userDataHex = '0x' + hex.encode(userData);

    return _interface.encodeFunctionData(
      'joinLottery',
      [thingIdHex, userDataHex],
    );
  }
}
