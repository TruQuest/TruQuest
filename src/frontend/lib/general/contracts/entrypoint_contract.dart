import '../../ethereum/models/im/user_operation.dart';
import '../../ethereum_js_interop.dart';

class EntryPointContract {
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

  final String address;
  late final Contract _readOnlyContract;

  EntryPointContract(this.address) {
    var provider = JsonRpcProvider('https://goerli.optimism.io');
    _readOnlyContract = Contract(
      address,
      _abi,
      provider,
    );
  }

  Future<BigInt> getNonce(String sender, int key) =>
      _readOnlyContract.read<BigInt>(
        'getNonce',
        args: [sender, key],
      );

  Future<String> getUserOpHash(UserOperation userOp) =>
      _readOnlyContract.read<String>(
        'getUserOpHash',
        args: [userOp.toList()],
      );
}
