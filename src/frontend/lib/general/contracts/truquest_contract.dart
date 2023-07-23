import 'package:convert/convert.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../extensions/uuid_extension.dart';
import '../../ethereum_js_interop.dart';

class TruQuestContract {
  static const String address = '0x3CD0E37bA3804cb84c2B0061978147011C18eAd3';
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

  late final Abi _interface;
  late final Contract _contract;

  TruQuestContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(address, _abi, ethereumRpcProvider.provider);
  }

  Future<bool> checkThingAlreadyFunded(String thingId) => _contract.read<bool>(
        'checkThingAlreadyFunded',
        args: [thingId.toSolInputFormat()],
      );

  String fundThing(String thingId, String signature) {
    var thingIdHex = thingId.toSolInputFormat();
    signature = signature.substring(2);
    var r = '0x' + signature.substring(0, 64);
    var s = '0x' + signature.substring(64, 128);
    var v = hex.decode(signature.substring(128, 130)).first;

    return _interface.encodeFunctionData(
      'fundThing',
      [
        [thingIdHex],
        v,
        r,
        s,
      ],
    );
  }

  Future<bool> checkThingAlreadyHasSettlementProposalUnderAssessment(
    String thingId,
  ) =>
      _contract.read<bool>(
        'checkThingAlreadyHasSettlementProposalUnderAssessment',
        args: [thingId.toSolInputFormat()],
      );

  String fundThingSettlementProposal(
    String thingId,
    String proposalId,
    String signature,
  ) {
    var thingIdHex = thingId.toSolInputFormat();
    var proposalIdHex = proposalId.toSolInputFormat();
    signature = signature.substring(2);
    var r = '0x' + signature.substring(0, 64);
    var s = '0x' + signature.substring(64, 128);
    var v = hex.decode(signature.substring(128, 130)).first;

    return _interface.encodeFunctionData(
      'fundThingSettlementProposal',
      [
        [thingIdHex, proposalIdHex],
        v,
        r,
        s,
      ],
    );
  }

  String depositFunds(int amount) {
    return _interface.encodeFunctionData(
      'deposit',
      [BigNumber.from(amount.toString())],
    );
  }
}
