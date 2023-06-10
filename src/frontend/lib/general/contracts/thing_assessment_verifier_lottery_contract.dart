import 'dart:math';

import 'package:convert/convert.dart';
import 'package:flutter_web3/flutter_web3.dart';

import '../../ethereum/errors/ethereum_error.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../extensions/uuid_extension.dart';

class ThingAssessmentVerifierLotteryContract {
  static const String _address = '0x5075a5F27c933F1e2eeB4AD5329B79EF454F619f';
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
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
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

  final EthereumService _ethereumService;

  final _random = Random.secure();

  late final Contract? _contract;

  ThingAssessmentVerifierLotteryContract(this._ethereumService) {
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

  Future<int> getLotteryInitBlock(String thingId, String proposalId) async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var initBlock = await contract.call<BigInt>(
      'getLotteryInitBlock',
      [thingProposalIdHex],
    );

    return initBlock.toInt();
  }

  Future<String?> getOrchestratorCommitmentDataHash(
    String thingId,
    String proposalId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return await contract.call<String>(
      'getOrchestratorCommitmentDataHash',
      [thingProposalIdHex],
    );
  }

  Future<String?> getOrchestratorCommitmentUserXorDataHash(
    String thingId,
    String proposalId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return await contract.call<String>(
      'getOrchestratorCommitmentUserXorDataHash',
      [thingProposalIdHex],
    );
  }

  Future<bool?> checkAlreadyClaimedLotterySpot(
    String thingId,
    String proposalId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }
    if (_ethereumService.connectedAccount == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return await contract.call<bool>(
      'checkAlreadyClaimedLotterySpot',
      [
        thingProposalIdHex,
        _ethereumService.connectedAccount,
      ],
    );
  }

  Future<bool?> checkAlreadyJoinedLottery(
    String thingId,
    String proposalId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return null;
    }
    if (_ethereumService.connectedAccount == null) {
      return null;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return await contract.call<bool>(
      'checkAlreadyJoinedLottery',
      [
        thingProposalIdHex,
        _ethereumService.connectedAccount,
      ],
    );
  }

  Future<EthereumError?> claimLotterySpot(
    String thingId,
    String proposalId,
    int userIndexInThingVerifiersArray,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return EthereumError('Metamask not installed');
    }
    if (_ethereumService.connectedAccount == null) {
      return EthereumError('No account connected');
    }

    var signer = _ethereumService.provider.getSigner();
    contract = contract.connect(signer);

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    try {
      var txnResponse = await contract.send(
        'claimLotterySpot',
        [thingProposalIdHex, userIndexInThingVerifiersArray],
        TransactionOverride(
          gasLimit: BigInt.from(250000),
        ),
      );

      await txnResponse.wait();
      print('Claim txn mined!');

      return null;
    } catch (e) {
      print(e);
      return EthereumError(e.toString());
    }
  }

  Future<EthereumError?> joinLottery(String thingId, String proposalId) async {
    var contract = _contract;
    if (contract == null) {
      return EthereumError('Metamask not installed');
    }
    if (_ethereumService.connectedAccount == null) {
      return EthereumError('No account connected');
    }

    var signer = _ethereumService.provider.getSigner();
    contract = contract.connect(signer);

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var userData = List<int>.generate(32, (_) => _random.nextInt(256));
    var userDataHex = '0x' + hex.encode(userData);

    try {
      var txnResponse = await contract.send(
        'joinLottery',
        [thingProposalIdHex, userDataHex],
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
