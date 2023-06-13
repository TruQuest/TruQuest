import '../../ethereum/services/ethereum_service.dart';
import '../../ethereum_js_interop.dart';
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
          "internalType": "int256",
          "name": "",
          "type": "int256"
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

    return await contract.read<int>('getPollDurationBlocks');
  }

  Future<int> getPollInitBlock(String thingId, String proposalId) async {
    var contract = _contract;
    if (contract == null) {
      return 0;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return (await contract.read<BigInt>(
      'getPollInitBlock',
      args: [thingProposalIdHex],
    ))
        .toInt();
  }

  Future<int> getUserIndexAmongProposalVerifiers(
    String thingId,
    String proposalId,
  ) async {
    var contract = _contract;
    if (contract == null) {
      return -1;
    }
    if (_ethereumService.connectedAccount == null) {
      return -1;
    }

    var thingIdHex = thingId.toSolInputFormat(prefix: false);
    var proposalIdHex = proposalId.toSolInputFormat(prefix: false);
    var thingProposalIdHex = '0x' + thingIdHex + proposalIdHex;

    return (await contract.read<BigInt>(
      'getUserIndexAmongProposalVerifiers',
      args: [
        thingProposalIdHex,
        _ethereumService.connectedAccount,
      ],
    ))
        .toInt();
  }

  Future castVote(
    String thingId,
    String proposalId,
    int userIndexInProposalVerifiersArray,
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
        txnResponse = await contract.write(
          'castVote',
          args: [
            thingProposalIdHex,
            userIndexInProposalVerifiersArray,
            decision.index,
          ],
        );
      } else {
        txnResponse = await contract.write(
          'castVoteWithReason',
          args: [
            thingProposalIdHex,
            userIndexInProposalVerifiersArray,
            decision.index,
            reason,
          ],
        );
      }

      await txnResponse.wait();
      print('Cast vote txn mined!');
    } on ContractRequestError catch (e) {
      print('Cast vote error: [${e.code}] ${e.message}');
    } on ContractExecError catch (e) {
      print('Cast vote error: [${e.code}] ${e.reason}');
    }
  }
}
