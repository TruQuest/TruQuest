import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'base_contract.dart';
import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';

class TruthserumContract extends BaseContract {
  static final String _address = dotenv.env['TruthserumAddress']!;
  static const String _abi = '''[
    {
      "inputs": [
        {
          "internalType": "address",
          "name": "account",
          "type": "address"
        }
      ],
      "name": "balanceOf",
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
          "name": "spender",
          "type": "address"
        },
        {
          "internalType": "uint256",
          "name": "amount",
          "type": "uint256"
        }
      ],
      "name": "approve",
      "outputs": [
        {
          "internalType": "bool",
          "name": "",
          "type": "bool"
        }
      ],
      "stateMutability": "nonpayable",
      "type": "function"
    }
  ]''';

  static final String _truQuestAddress = dotenv.env['TruQuestAddress']!;

  TruthserumContract(EthereumRpcProvider ethereumRpcProvider) : super(_address, _abi, ethereumRpcProvider.provider);

  Future<BigInt> balanceOf(String address) => contract.read<BigInt>('balanceOf', args: [address]);

  String approve(int amount) {
    return interface.encodeFunctionData(
      'approve',
      [_truQuestAddress, BigNumber.from(amount.toString())],
    );
  }
}
