import 'package:convert/convert.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../utils/utils.dart';
import '../../ethereum_js_interop.dart';

class TruQuestContract {
  static final String address = dotenv.env['TruQuestAddress']!;
  static const String _abi = '''[
        {
          "inputs": [],
          "name": "s_thingSubmissionStake",
          "outputs": [
            {
              "internalType": "uint256",
              "name": "",
              "type": "uint256"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
        {
          "inputs": [],
          "name": "s_thingSettlementProposalStake",
          "outputs": [
            {
              "internalType": "uint256",
              "name": "",
              "type": "uint256"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
        {
          "inputs": [],
          "name": "s_verifierStake",
          "outputs": [
            {
              "internalType": "uint256",
              "name": "",
              "type": "uint256"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
        {
          "inputs": [
            {
              "internalType": "address",
              "name": "",
              "type": "address"
            }
          ],
          "name": "s_balanceOf",
          "outputs": [
            {
              "internalType": "uint256",
              "name": "",
              "type": "uint256"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
        {
          "inputs": [
            {
              "internalType": "address",
              "name": "",
              "type": "address"
            }
          ],
          "name": "s_stakedBalanceOf",
          "outputs": [
            {
              "internalType": "uint256",
              "name": "",
              "type": "uint256"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
        {
          "inputs": [
            {
              "internalType": "address",
              "name": "_user",
              "type": "address"
            }
          ],
          "name": "getAvailableFunds",
          "outputs": [
            {
              "internalType": "uint256",
              "name": "",
              "type": "uint256"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
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
              "internalType": "uint256",
              "name": "_amount",
              "type": "uint256"
            }
          ],
          "name": "withdraw",
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
              "internalType": "bytes16",
              "name": "_thingId",
              "type": "bytes16"
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
            },
            {
              "internalType": "bytes16",
              "name": "_proposalId",
              "type": "bytes16"
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
        }
      ]''';

  late final Abi _interface;
  late final Contract _contract;

  TruQuestContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(address, _abi, ethereumRpcProvider.provider);
  }

  Future<BigInt> getThingSubmissionStake() => _contract.read<BigInt>('s_thingSubmissionStake');

  Future<BigInt> getThingSettlementProposalStake() => _contract.read<BigInt>('s_thingSettlementProposalStake');

  Future<BigInt> getVerifierStake() => _contract.read<BigInt>('s_verifierStake');

  Future<BigInt> balanceOf(String address) => _contract.read<BigInt>('s_balanceOf', args: [address]);

  Future<BigInt> stakedBalanceOf(String address) => _contract.read<BigInt>('s_stakedBalanceOf', args: [address]);

  Future<BigInt> getAvailableFunds(String address) => _contract.read<BigInt>('getAvailableFunds', args: [address]);

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
        thingIdHex,
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
        thingIdHex,
        proposalIdHex,
        v,
        r,
        s,
      ],
    );
  }

  String depositFunds(int amount) => _interface.encodeFunctionData(
        'deposit',
        [BigNumber.from(amount.toString())],
      );

  String withdrawFunds(int amount) => _interface.encodeFunctionData(
        'withdraw',
        [BigNumber.from(amount.toString())],
      );
}
