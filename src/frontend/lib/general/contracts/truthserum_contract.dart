import '../../ethereum/errors/ethereum_error.dart';
import '../../ethereum_js_interop.dart';
import 'truquest_contract.dart';
import '../../ethereum/services/ethereum_service.dart';

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

  final EthereumService _ethereumService;

  late final Contract? _contract;

  TruthserumContract(this._ethereumService) {
    if (_ethereumService.isAvailable) {
      _contract = Contract(_address, _abi, _ethereumService.provider);
    }
  }

  Future<EthereumError?> approve(int amount) async {
    var contract = _contract;
    if (contract == null) {
      return EthereumError('Metamask not installed');
    }
    var connectedAccount = _ethereumService.connectedAccount;
    if (connectedAccount == null) {
      return EthereumError('No account connected');
    }

    try {
      var balance = await contract.read<BigInt>(
        'balanceOf',
        args: [connectedAccount],
      );

      print('Balance: $balance');

      if (balance < BigInt.from(amount)) {
        return EthereumError('Not enough funds');
      }

      var signer = _ethereumService.provider.getSigner();
      contract = contract.connect(signer);

      var txnResponse = await contract.write(
        'approve',
        args: [
          TruQuestContract.address,
          BigInt.from(amount),
        ],
        override: TransactionOverride(
          gasLimit: 150000,
        ),
      );

      await txnResponse.wait();
      print('Approve usage txn mined!');

      return null;
    } on ContractRequestError catch (e) {
      print('Approve funds usage error: [${e.code}] ${e.message}');
      return EthereumError('Error approving usage of funds');
    } on ContractExecError catch (e) {
      print('Approve funds usage error: [${e.code}] ${e.reason}');
      return EthereumError('Error approving usage of funds');
    }
  }
}
