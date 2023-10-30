import 'dart:math';

import 'package:convert/convert.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';
import '../utils/utils.dart';

class SettlementProposalAssessmentVerifierLotteryContract {
  static final String address = dotenv.env['SettlementProposalAssessmentVerifierLotteryAddress']!;
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
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
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
      "name": "checkAlreadyClaimedLotterySpot",
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
          "name": "_thingProposalId",
          "type": "bytes32"
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
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
        },
        {
          "internalType": "uint16",
          "name": "_thingVerifiersArrayIndex",
          "type": "uint16"
        }
      ],
      "name": "claimLotterySpot",
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

  SettlementProposalAssessmentVerifierLotteryContract(
    EthereumRpcProvider ethereumRpcProvider,
  ) {
    _interface = Abi(_abi);
    _contract = Contract(
      address,
      _abi,
      ethereumRpcProvider.provider,
    );
  }

  Future<int> getLotteryDurationBlocks() => _contract.read<int>('s_durationBlocks');

  Future<int?> getLotteryInitBlock(String thingId, String proposalId) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var block = await _contract.read<BigInt>(
      'getLotteryInitBlock',
      args: [thingProposalIdHex],
    );

    return block != BigInt.zero ? block.toInt() : null;
  }

  Future<bool> checkAlreadyClaimedLotterySpot(
    String thingId,
    String proposalId,
    String walletAddress,
  ) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return _contract.read<bool>(
      'checkAlreadyClaimedLotterySpot',
      args: [
        thingProposalIdHex,
        walletAddress,
      ],
    );
  }

  Future<bool> checkAlreadyJoinedLottery(
    String thingId,
    String proposalId,
    String walletAddress,
  ) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return _contract.read<bool>(
      'checkAlreadyJoinedLottery',
      args: [
        thingProposalIdHex,
        walletAddress,
      ],
    );
  }

  String claimLotterySpot(
    String thingId,
    String proposalId,
    int thingVerifiersArrayIndex,
  ) {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return _interface.encodeFunctionData(
      'claimLotterySpot',
      [thingProposalIdHex, thingVerifiersArrayIndex],
    );
  }

  String joinLottery(String thingId, String proposalId) {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var userData = List<int>.generate(32, (_) => _random.nextInt(256));
    var userDataHex = '0x' + hex.encode(userData);

    return _interface.encodeFunctionData(
      'joinLottery',
      [thingProposalIdHex, userDataHex],
    );
  }
}
