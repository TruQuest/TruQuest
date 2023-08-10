import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';
import '../../settlement/models/im/decision_im.dart';
import '../utils/utils.dart';

class AssessmentPollContract {
  static final String address = dotenv.env['AssessmentPollAddress']!;
  static const String _abi = '''[
    {
      "inputs": [
        {
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
        },
        {
          "internalType": "uint16",
          "name": "_proposalVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum AssessmentPoll.Vote",
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
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
        },
        {
          "internalType": "uint16",
          "name": "_proposalVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum AssessmentPoll.Vote",
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
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
        },
        {
          "internalType": "address",
          "name": "_user",
          "type": "address"
        }
      ],
      "name": "getUserIndexAmongProposalVerifiers",
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
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
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
    }
  ]''';

  late final Abi _interface;
  late final Contract _contract;

  AssessmentPollContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(
      address,
      _abi,
      ethereumRpcProvider.provider,
    );
  }

  Future<int> getPollDurationBlocks() => _contract.read<int>('s_durationBlocks');

  Future<int?> getPollInitBlock(String thingId, String proposalId) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var block = await _contract.read<BigInt>(
      'getPollInitBlock',
      args: [thingProposalIdHex],
    );

    return block != BigInt.zero ? block.toInt() : null;
  }

  Future<int> getUserIndexAmongProposalVerifiers(
    String thingId,
    String proposalId,
    String walletAddress,
  ) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var index = await _contract.read<BigInt>(
      'getUserIndexAmongProposalVerifiers',
      args: [
        thingProposalIdHex,
        walletAddress,
      ],
    );

    return index.toInt();
  }

  String castVote(
    String thingId,
    String proposalId,
    int userIndexInProposalVerifiersArray,
    DecisionIm decision,
    String reason,
  ) {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return reason.isEmpty
        ? _interface.encodeFunctionData(
            'castVote',
            [
              thingProposalIdHex,
              userIndexInProposalVerifiersArray,
              decision.index,
            ],
          )
        : _interface.encodeFunctionData(
            'castVoteWithReason',
            [
              thingProposalIdHex,
              userIndexInProposalVerifiersArray,
              decision.index,
              reason,
            ],
          );
  }
}
