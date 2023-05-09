import 'package:flutter_web3/flutter_web3.dart';

import '../../ethereum/services/ethereum_service.dart';
import '../../settlement/models/im/decision_im.dart';
import '../extensions/uuid_extension.dart';

class AssessmentPollContract {
  static const String _address = '0x9E9642eD983aD7E5c4eDdb6c287DB05C96Ef45f8';
  static const String _abi = '''[
    {
      "inputs": [
        {
          "internalType": "bytes32",
          "name": "_thingProposalId",
          "type": "bytes32"
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
      "name": "checkIsDesignatedVerifierForProposal",
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
      "inputs": [],
      "name": "getPollDurationBlocks",
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
          "internalType": "uint64",
          "name": "",
          "type": "uint64"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    }
  ]''';

  final EthereumService _ethereumService;

  late final Contract? _contract;

  AssessmentPollContract(this._ethereumService) {
    if (_ethereumService.available) {
      _contract = Contract(_address, _abi, _ethereumService.provider);
    }
  }

  Future<int> getPollDurationBlocks() async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    return await contract.call<int>('getPollDurationBlocks');
  }

  Future<int> getPollInitBlock(String thingId, String proposalId) async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    var initBlock = await contract.call<BigInt>(
      'getPollInitBlock',
      [thingProposalIdHex],
    );

    return initBlock.toInt();
  }

  Future<bool?> checkIsDesignatedVerifierForProposal(
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
      'checkIsDesignatedVerifierForProposal',
      [
        thingProposalIdHex,
        _ethereumService.connectedAccount,
      ],
    );
  }

  Future castVote(
    String thingId,
    String proposalId,
    DecisionIm decision,
    String reason,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return;
    }
    if (_ethereumService.connectedAccount == null) {
      return;
    }

    var signer = _ethereumService.provider.getSigner();
    contract = contract.connect(signer);

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    try {
      TransactionResponse txnResponse;
      if (reason.isEmpty) {
        txnResponse = await contract.send(
          'castVote',
          [
            thingProposalIdHex,
            decision.index,
          ],
        );
      } else {
        txnResponse = await contract.send(
          'castVoteWithReason',
          [
            thingProposalIdHex,
            decision.index,
            reason,
          ],
        );
      }

      // print('Vote casted! Awaiting confirmation...');

      // await txnResponse.wait(2);

      // print('Cast vote txn confirmed!');

      await txnResponse.wait();
      print('Cast vote txn mined!');
    } catch (e) {
      print(e);
    }
  }
}
