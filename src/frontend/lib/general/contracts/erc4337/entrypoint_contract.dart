import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../../ethereum/services/ethereum_rpc_provider.dart';
import 'ientrypoint_contract.dart';
import '../../../ethereum/models/im/user_operation.dart';
import '../../../ethereum_js_interop.dart';

class EntryPointContract implements IEntryPointContract {
  static const String _abi = '''[
    {
      "inputs": [
        {
          "internalType": "address",
          "name": "sender",
          "type": "address"
        },
        {
          "internalType": "uint192",
          "name": "key",
          "type": "uint192"
        }
      ],
      "name": "getNonce",
      "outputs": [
        {
          "internalType": "uint256",
          "name": "nonce",
          "type": "uint256"
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
              "internalType": "address",
              "name": "sender",
              "type": "address"
            },
            {
              "internalType": "uint256",
              "name": "nonce",
              "type": "uint256"
            },
            {
              "internalType": "bytes",
              "name": "initCode",
              "type": "bytes"
            },
            {
              "internalType": "bytes",
              "name": "callData",
              "type": "bytes"
            },
            {
              "internalType": "uint256",
              "name": "callGasLimit",
              "type": "uint256"
            },
            {
              "internalType": "uint256",
              "name": "verificationGasLimit",
              "type": "uint256"
            },
            {
              "internalType": "uint256",
              "name": "preVerificationGas",
              "type": "uint256"
            },
            {
              "internalType": "uint256",
              "name": "maxFeePerGas",
              "type": "uint256"
            },
            {
              "internalType": "uint256",
              "name": "maxPriorityFeePerGas",
              "type": "uint256"
            },
            {
              "internalType": "bytes",
              "name": "paymasterAndData",
              "type": "bytes"
            },
            {
              "internalType": "bytes",
              "name": "signature",
              "type": "bytes"
            }
          ],
          "internalType": "struct UserOperation",
          "name": "userOp",
          "type": "tuple"
        }
      ],
      "name": "getUserOpHash",
      "outputs": [
        {
          "internalType": "bytes32",
          "name": "",
          "type": "bytes32"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    }
  ]''';

  late final Contract _contract;

  late final String _address = dotenv.env['EntryPointAddress']!;
  @override
  String get address => _address;

  EntryPointContract(EthereumRpcProvider ethereumRpcProvider) {
    _contract = Contract(
      address,
      _abi,
      ethereumRpcProvider.provider,
    );
  }

  @override
  Future<BigInt> getNonce(String sender) => _contract.read<BigInt>(
        'getNonce',
        args: [sender, 0],
      );

  @override
  Future<String> getUserOpHash(UserOperation userOp) => _contract.read<String>(
        'getUserOpHash',
        args: [userOp.toList()],
      );
}
