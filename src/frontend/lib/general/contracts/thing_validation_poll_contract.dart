import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../thing/models/im/decision_im.dart';
import '../utils/utils.dart';
import 'base_contract.dart';

class ThingValidationPollContract extends BaseContract {
  static final String _address = dotenv.env['ThingValidationPollAddress']!;
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
          "internalType": "uint16",
          "name": "_thingVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum ThingValidationPoll.Vote",
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
          "internalType": "uint16",
          "name": "_thingVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum ThingValidationPoll.Vote",
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
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "thingId",
          "type": "bytes16"
        }
      ],
      "name": "ThingValidationPoll__Expired",
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
      "name": "ThingValidationPoll__NotActive",
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
      "name": "ThingValidationPoll__NotDesignatedVerifier",
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
      "name": "ThingValidationPoll__StillInProgress",
      "type": "error"
    },
    {
      "inputs": [],
      "name": "ThingValidationPoll__Unauthorized",
      "type": "error"
    }
  ]''';

  ThingValidationPollContract(EthereumRpcProvider ethereumRpcProvider)
      : super(_address, _abi, ethereumRpcProvider.provider);

  Future<int> getPollDurationBlocks() => contract.read<int>('s_durationBlocks');

  Future<int?> getPollInitBlock(String thingId) async {
    var initBlock = await contract.read<BigInt>(
      'getPollInitBlock',
      args: [thingId.toSolInputFormat()],
    );
    return initBlock != BigInt.zero ? initBlock.toInt() : null;
  }

  String castVote(
    String thingId,
    int thingVerifiersArrayIndex,
    DecisionIm decision,
    String reason,
  ) {
    var thingIdHex = thingId.toSolInputFormat();
    return reason.isEmpty
        ? interface.encodeFunctionData(
            'castVote',
            [
              thingIdHex,
              thingVerifiersArrayIndex,
              decision.index,
            ],
          )
        : interface.encodeFunctionData(
            'castVoteWithReason',
            [
              thingIdHex,
              thingVerifiersArrayIndex,
              decision.index,
              reason,
            ],
          );
  }

  Future<int> getUserIndexAmongThingVerifiers(
    String thingId,
    String walletAddress,
  ) async {
    var index = await contract.read<BigInt>(
      'getUserIndexAmongThingVerifiers',
      args: [thingId.toSolInputFormat(), walletAddress],
    );
    return index.toInt();
  }
}
