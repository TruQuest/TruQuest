import 'package:convert/convert.dart';

import '../../ethereum/errors/ethereum_error.dart';
import '../../ethereum/services/ethereum_service.dart';
import '../../ethereum_js_interop.dart';
import '../extensions/uuid_extension.dart';

class TruQuestContract {
  static const String address = '0x32D41E4e24F97ec7D52e3c43F8DbFe209CBd0e4c';
  static const String _abi = '''[
        {
          "inputs": [
            {
              "internalType": "uint256",
              "name": "_amount",
              "type": "uint256"
            }
          ],
          "name": "deposit",
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
            }
          ],
          "name": "checkThingAlreadyFunded",
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
              "components": [
                {"internalType": "bytes16", "name": "id", "type": "bytes16"}
              ],
              "internalType": "struct TruQuest.ThingTd",
              "name": "_thing",
              "type": "tuple"
            },
            {"internalType": "uint8", "name": "_v", "type": "uint8"},
            {"internalType": "bytes32", "name": "_r", "type": "bytes32"},
            {"internalType": "bytes32", "name": "_s", "type": "bytes32"}
          ],
          "name": "fundThing",
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
            }
          ],
          "name": "checkThingAlreadyHasSettlementProposalUnderAssessment",
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
              "components": [
                {
                  "internalType": "bytes16",
                  "name": "thingId",
                  "type": "bytes16"
                },
                {
                  "internalType": "bytes16",
                  "name": "id",
                  "type": "bytes16"
                }
              ],
              "internalType": "struct TruQuest.SettlementProposalTd",
              "name": "_settlementProposal",
              "type": "tuple"
            },
            {
              "internalType": "uint8",
              "name": "_v",
              "type": "uint8"
            },
            {
              "internalType": "bytes32",
              "name": "_r",
              "type": "bytes32"
            },
            {
              "internalType": "bytes32",
              "name": "_s",
              "type": "bytes32"
            }
          ],
          "name": "fundThingSettlementProposal",
          "outputs": [],
          "stateMutability": "nonpayable",
          "type": "function"
        }
      ]''';

  final EthereumService _ethereumService;

  late final Contract? _contract;
  late final Contract _readOnlyContract;

  TruQuestContract(this._ethereumService) {
    _readOnlyContract = Contract(
      address,
      _abi,
      _ethereumService.l2ReadOnlyProvider,
    );
    if (_ethereumService.isAvailable) {
      _contract = Contract(address, _abi, _ethereumService.provider);
    }
  }

  Future<bool> checkThingAlreadyFunded(String thingId) async {
    var thingIdHex = thingId.toSolInputFormat();

    return await _readOnlyContract.read<bool>(
      'checkThingAlreadyFunded',
      args: [thingIdHex],
    );
  }

  Future fundThing(String thingId, String signature) async {
    var contract = _contract;
    if (contract == null) {
      return;
    }
    if (_ethereumService.connectedAccount == null) {
      return;
    }

    var signer =
        _ethereumService.provider.getSigner(); // @@??: What if not connected ?
    contract = contract.connect(signer);

    var thingIdHex = thingId.toSolInputFormat();
    signature = signature.substring(2);
    var r = '0x' + signature.substring(0, 64);
    var s = '0x' + signature.substring(64, 128);
    var v = hex.decode(signature.substring(128, 130)).first;

    try {
      var txnResponse = await contract.write(
        'fundThing',
        args: [
          [thingIdHex],
          v,
          r,
          s,
        ],
        override: TransactionOverride(
          gasLimit: 100000, // 83397
        ),
      );

      await txnResponse.wait();
      print('Fund txn mined!');
    } on ContractRequestError catch (e) {
      print('Fund thing error: [${e.code}] ${e.message}');
    } on ContractExecError catch (e) {
      print('Fund thing error: [${e.code}] ${e.reason}');
    }
  }

  Future<bool> checkThingAlreadyHasSettlementProposalUnderAssessment(
    String thingId,
  ) async {
    var thingIdHex = thingId.toSolInputFormat();

    return await _readOnlyContract.read<bool>(
      'checkThingAlreadyHasSettlementProposalUnderAssessment',
      args: [thingIdHex],
    );
  }

  Future fundThingSettlementProposal(
    String thingId,
    String proposalId,
    String signature,
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

    var thingIdHex = thingId.toSolInputFormat();
    var proposalIdHex = proposalId.toSolInputFormat();
    signature = signature.substring(2);
    var r = '0x' + signature.substring(0, 64);
    var s = '0x' + signature.substring(64, 128);
    var v = hex.decode(signature.substring(128, 130)).first;

    try {
      var txnResponse = await contract.write(
        'fundThingSettlementProposal',
        args: [
          [thingIdHex, proposalIdHex],
          v,
          r,
          s,
        ],
        override: TransactionOverride(
          gasLimit: 150000,
        ),
      );

      await txnResponse.wait();
      print('Fund txn mined!');
    } on ContractRequestError catch (e) {
      print('Fund proposal error: [${e.code}] ${e.message}');
    } on ContractExecError catch (e) {
      print('Fund proposal error: [${e.code}] ${e.reason}');
    }
  }

  Future<EthereumError?> depositFunds(int amount) async {
    var contract = _contract;
    if (contract == null) {
      return EthereumError('Metamask not installed');
    }
    if (_ethereumService.connectedAccount == null) {
      return EthereumError('No account connected');
    }

    var signer = _ethereumService.provider.getSigner();
    contract = contract.connect(signer);

    try {
      var txnResponse = await contract.write(
        'deposit',
        args: [BigInt.from(amount)],
        override: TransactionOverride(
          gasLimit: 150000,
        ),
      );

      await txnResponse.wait();
      print('Deposit funds txn mined!');

      return null;
    } on ContractRequestError catch (e) {
      print('Deposit funds error: [${e.code}] ${e.message}');
      return EthereumError('Error depositing funds');
    } on ContractExecError catch (e) {
      print('Deposit funds error: [${e.code}] ${e.reason}');
      return EthereumError('Error depositing funds');
    }
  }
}
