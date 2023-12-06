import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'iaccount_contract.dart';
import 'light_account_contract.dart';
import 'iaccount_factory_contract.dart';
import '../../../ethereum_js_interop.dart';
import '../../../ethereum/services/ethereum_rpc_provider.dart';

class LightAccountFactoryContract implements IAccountFactoryContract {
  static final String _address = dotenv.env['LightAccountFactoryAddress']!;
  static const String _abi = '''
  [
    {
        "inputs": [],
        "name": "accountImplementation",
        "outputs": [
            {
                "internalType": "contract LightAccount",
                "name": "",
                "type": "address"
            }
        ],
        "stateMutability": "view",
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
        "name": "createAccount",
        "outputs": [
            {
                "internalType": "contract LightAccount",
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

  late final Abi _interface;
  late final Contract _contract;

  @override
  IAccountContract get accountContract => LightAccountContract.instance;

  @override
  String get dummySignatureForGasEstimation =>
      '0xfffffffffffffffffffffffffffffff0000000000000000000000000000000007aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa1c';

  LightAccountFactoryContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(_address, _abi, ethereumRpcProvider.provider);
  }

  @override
  Future<String> getAddress(String signerAddress) async =>
      convertToEip55Address(await _contract.read<String>('getAddress', args: [signerAddress, 0]));

  @override
  String getInitCode(String signerAddress) =>
      _address + _interface.encodeFunctionData('createAccount', [signerAddress, 0]).substring(2);
}
