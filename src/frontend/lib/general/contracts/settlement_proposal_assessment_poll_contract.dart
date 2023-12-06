import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../settlement/models/im/decision_im.dart';
import '../utils/utils.dart';
import 'base_contract.dart';

class SettlementProposalAssessmentPollContract extends BaseContract {
  static final String _address = dotenv.env['SettlementProposalAssessmentPollAddress']!;
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
          "name": "_settlementProposalVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum SettlementProposalAssessmentPoll.Vote",
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
          "name": "_settlementProposalVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum SettlementProposalAssessmentPoll.Vote",
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
      "name": "getUserIndexAmongSettlementProposalVerifiers",
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
    },
    {
      "inputs": [
        {
          "internalType": "bytes32",
          "name": "thingProposalId",
          "type": "bytes32"
        }
      ],
      "name": "SettlementProposalAssessmentPoll__Expired",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes32",
          "name": "thingProposalId",
          "type": "bytes32"
        }
      ],
      "name": "SettlementProposalAssessmentPoll__NotActive",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes32",
          "name": "thingProposalId",
          "type": "bytes32"
        }
      ],
      "name": "SettlementProposalAssessmentPoll__NotDesignatedVerifier",
      "type": "error"
    },
    {
      "inputs": [
        {
          "internalType": "bytes32",
          "name": "thingProposalId",
          "type": "bytes32"
        }
      ],
      "name": "SettlementProposalAssessmentPoll__StillInProgress",
      "type": "error"
    },
    {
      "inputs": [],
      "name": "SettlementProposalAssessmentPoll__Unauthorized",
      "type": "error"
    }
  ]''';

  SettlementProposalAssessmentPollContract(EthereumRpcProvider ethereumRpcProvider)
      : super(_address, _abi, ethereumRpcProvider.provider);

  Future<int> getPollDurationBlocks() => contract.read<int>('s_durationBlocks');

  Future<int?> getPollInitBlock(String thingId, String proposalId) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var block = await contract.read<BigInt>(
      'getPollInitBlock',
      args: [thingProposalIdHex],
    );

    return block != BigInt.zero ? block.toInt() : null;
  }

  Future<int> getUserIndexAmongSettlementProposalVerifiers(
    String thingId,
    String proposalId,
    String walletAddress,
  ) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var index = await contract.read<BigInt>(
      'getUserIndexAmongSettlementProposalVerifiers',
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
    int settlementProposalVerifiersArrayIndex,
    DecisionIm decision,
    String reason,
  ) {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return reason.isEmpty
        ? interface.encodeFunctionData(
            'castVote',
            [
              thingProposalIdHex,
              settlementProposalVerifiersArrayIndex,
              decision.index,
            ],
          )
        : interface.encodeFunctionData(
            'castVoteWithReason',
            [
              thingProposalIdHex,
              settlementProposalVerifiersArrayIndex,
              decision.index,
              reason,
            ],
          );
  }
}
