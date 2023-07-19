import 'truquest_contract.dart';
import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';

class TruthserumContract {
  static const String _address = '0x19CFc85e3dffb66295695Bf48e06386CB1B5f320';
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

  late final Abi _interface;
  late final Contract _contract;

  TruthserumContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(_address, _abi, ethereumRpcProvider.provider);
  }

  (String, String) approve(int amount) {
    return (
      _address,
      _interface.encodeFunctionData(
        'approve',
        [
          TruQuestContract.address,
          BigInt.from(amount),
        ],
      ),
    );
  }
}
