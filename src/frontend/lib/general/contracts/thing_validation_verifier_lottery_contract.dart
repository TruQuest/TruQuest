import 'dart:math';

import 'package:convert/convert.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';
import '../utils/utils.dart';

class ThingValidationVerifierLotteryContract {
  static final String address = dotenv.env['ThingValidationVerifierLotteryAddress']!;
  static const String _abi = '''[
    {
      "inputs": [],
      "name": "s_durationBlocks",
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
    },
    {
      "inputs": [],
      "name": "RestrictedAccess__Forbidden",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__AlreadyInitialized",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__AlreadyJoined",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__Expired",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__InvalidNumberOfWinners",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__InvalidReveal",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__NotActive",
      "type": "error"
    },
    {
      "inputs": [],
      "name": "ThingValidationVerifierLottery__NotEnoughFunds",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__StillInProgress",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationVerifierLottery__SubmitterCannotParticipate",
      "type": "error"
    },
    {
      "inputs": [],
      "name": "ThingValidationVerifierLottery__Unauthorized",
      "type": "error"
    }
  ]''';

  final _random = Random.secure();

  late final Abi _interface;
  late final Contract _contract;

  ThingValidationVerifierLotteryContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(
      address,
      _abi,
      ethereumRpcProvider.provider,
    );
  }

  ErrorDescription parseError(String data) => _interface.parseError(data);

  Future<int> getLotteryDurationBlocks() => _contract.read<int>('s_durationBlocks');

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
