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
      "name": "checkAlreadyClaimedALotterySpot",
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
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
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
          "name": "_dataHash",
          "type": "bytes32"
        }
      ],
      "name": "preJoinLottery",
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
          "name": "_data",
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

  final Map<String, String> _commitmentIdToData = {};

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

  Future<bool?> checkAlreadyClaimedALotterySpot(
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
      'checkAlreadyClaimedALotterySpot',
      [
        thingProposalIdHex,
        _ethereumService.connectedAccount,
      ],
    );
  }

  Future<bool?> checkAlreadyPreJoinedLottery(
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
      'checkAlreadyPreJoinedLottery',
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
        [thingProposalIdHex],
        TransactionOverride(
          gasLimit: BigInt.from(250000),
        ),
      );

      // print('Claimed lottery spot! Awaiting confirmations...');

      // await txnResponse.wait(2);

      // print('Claim txn confirmed!');

      await txnResponse.wait();
      print('Claim txn mined!');

      return null;
    } catch (e) {
      print(e);
      return EthereumError(e.toString());
    }
  }

  Future<EthereumError?> preJoinLottery(
    String thingId,
    String proposalId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return EthereumError('Metamask not installed');
    }
    if (_ethereumService.connectedAccount == null) {
      return EthereumError('No account connected');
    }

    var signer = _ethereumService.provider.getSigner();
    var address = await signer.getAddress();
    contract = contract.connect(signer);

    var data = List<int>.generate(32, (_) => _random.nextInt(256));
    var dataHex = '0x' + hex.encode(data);
    print('dataHex: $dataHex');
    var dataHashHex = await contract.call<String>('computeHash', [dataHex]);
    print('dataHashHex: $dataHashHex');

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    try {
      var txnResponse = await contract.send(
        'preJoinLottery',
        [thingProposalIdHex, dataHashHex],
        TransactionOverride(
          gasLimit: BigInt.from(250000),
        ),
      );

      // print('PreJoined lottery! Awaiting confirmations...');

      // await txnResponse.wait(2);

      // print('PreJoin txn confirmed!');

      await txnResponse.wait();
      print('PreJoin txn mined!');

      _commitmentIdToData['$thingId|$proposalId|$address'] = dataHex;

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
    var address = await signer.getAddress();
    contract = contract.connect(signer);

    if (!_commitmentIdToData.containsKey('$thingId|$proposalId|$address')) {
      return EthereumError('Not committed to the lottery');
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;
    var dataHex = _commitmentIdToData['$thingId|$proposalId|$address'];

    try {
      var txnResponse = await contract.send(
        'joinLottery',
        [thingProposalIdHex, dataHex],
        TransactionOverride(
          gasLimit: BigInt.from(250000),
        ),
      );

      // print('Joined lottery! Awaiting confirmations...');

      // await txnResponse.wait(2);

      // print('Join txn confirmed!');

      await txnResponse.wait();
      print('Join txn mined!');

      _commitmentIdToData.remove('$thingId|$proposalId|$address');

      return null;
    } catch (e) {
      print(e);
      return EthereumError(e.toString());
    }
  }
}
