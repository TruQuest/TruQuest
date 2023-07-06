import 'dart:math';

import 'package:convert/convert.dart';

import '../../ethereum/errors/ethereum_error.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../../ethereum_js_interop.dart';
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

  Contract? _contract;
  late final Contract _readOnlyContract;

  ThingAssessmentVerifierLotteryContract(this._ethereumService) {
    _readOnlyContract = Contract(
      _address,
      _abi,
      _ethereumService.l2ReadOnlyProvider,
    );

    if (_ethereumService.isAvailable) {
      if (!_ethereumService.multipleWalletsDetected ||
          _ethereumService.walletSelected.isCompleted) {
        _contract = Contract(_address, _abi, _ethereumService.provider);
      } else {
        _ethereumService.walletSelected.future.then((_) {
          _contract = Contract(_address, _abi, _ethereumService.provider);
        });
      }
    }
  }

  Future<int> getLotteryDurationBlocks() =>
      _readOnlyContract.read<int>('getLotteryDurationBlocks');

  Future<int> getLotteryInitBlock(String thingId, String proposalId) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return (await _readOnlyContract.read<BigInt>(
      'getLotteryInitBlock',
      args: [thingProposalIdHex],
    ))
        .toInt();
  }

  Future<(int, String, String)> getOrchestratorCommitment(
    String thingId,
    String proposalId,
  ) async {
    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var result = await _readOnlyContract.read<List<dynamic>>(
      'getOrchestratorCommitment',
      args: [thingProposalIdHex],
    );

    return (
      (result[0] as BigInt).toInt(),
      result[1] as String,
      result[2] as String
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

    return await contract.read<bool>(
      'checkAlreadyClaimedLotterySpot',
      args: [
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

    return await contract.read<bool>(
      'checkAlreadyJoinedLottery',
      args: [
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
      var txnResponse = await contract.write(
        'claimLotterySpot',
        args: [thingProposalIdHex, userIndexInThingVerifiersArray],
        override: TransactionOverride(
          gasLimit: 250000,
        ),
      );

      await txnResponse.wait();
      print('Claim txn mined!');

      return null;
    } on ContractRequestError catch (e) {
      print('Claim lottery spot error: [${e.code}] ${e.message}');
      return EthereumError('Error claiming lottery spot');
    } on ContractExecError catch (e) {
      print('Claim lottery spot error: [${e.code}] ${e.reason}');
      return EthereumError('Error claiming lottery spot');
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
      var txnResponse = await contract.write(
        'joinLottery',
        args: [thingProposalIdHex, userDataHex],
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
