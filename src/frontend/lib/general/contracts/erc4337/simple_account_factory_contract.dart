import '../../../ethereum_js_interop.dart';
import '../../../ethereum/services/ethereum_api_service.dart';

class SimpleAccountFactoryContract {
  static const String address = '0x9406Cc6185a346906296840746125a0E44976454';
  static const String _abi = '''
    [
        {
            "inputs": [
                {
                    "internalType": "address",
                    "name": "owner",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "salt",
                    "type": "uint256"
                }
            ],
            "name": "createAccount",
            "outputs": [
                {
                    "internalType": "contract SimpleAccount",
                    "name": "ret",
                    "type": "address"
                }
            ],
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "inputs": [
                {
                    "internalType": "address",
                    "name": "owner",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "salt",
                    "type": "uint256"
                }
            ],
            "name": "getAddress",
            "outputs": [
                {
                    "internalType": "address",
                    "name": "",
                    "type": "address"
                }
            ],
            "stateMutability": "view",
            "type": "function"
        }
    ]''';

  late final Contract _contract;

  SimpleAccountFactoryContract(EthereumApiService ethereumApiService) {
    _contract = Contract(address, _abi, ethereumApiService.provider);
  }

  Future<String> getAddress(String ownerAddress) async => convertToEip55Address(
        await _contract.read<String>(
          'getAddress',
          args: [ownerAddress, 0],
        ),
      );
}
