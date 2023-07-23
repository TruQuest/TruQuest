import 'truquest_contract.dart';
import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';

class TruthserumContract {
  static const String address = '0x10460A3DB94faEf76918269981541b60B4f6e1a5';
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
    _contract = Contract(address, _abi, ethereumRpcProvider.provider);
  }

  Future<int> balanceOf(String address) async {
    var balance = await _contract.read<BigInt>('balanceOf', args: [address]);
    return balance.toInt();
  }

  String approve(int amount) {
    return _interface.encodeFunctionData(
      'approve',
      [
        TruQuestContract.address,
        BigNumber.from(amount.toString()),
      ],
    );
  }
}
