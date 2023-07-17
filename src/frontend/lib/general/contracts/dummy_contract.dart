import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';

class DummyContract {
  static const String _address = '0x19CFc85e3dffb66295695Bf48e06386CB1B5f320';
  static const String _abi = '''
    [
        {
          "inputs": [],
          "name": "getValue",
          "outputs": [
            {
              "internalType": "string",
              "name": "",
              "type": "string"
            }
          ],
          "stateMutability": "view",
          "type": "function"
        },
        {
          "inputs": [
            {
              "internalType": "string",
              "name": "_value",
              "type": "string"
            }
          ],
          "name": "setValue",
          "outputs": [],
          "stateMutability": "nonpayable",
          "type": "function"
        }
    ]''';

  late final Abi _interface;
  late final Contract _contract;

  DummyContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(_address, _abi, ethereumRpcProvider.provider);
  }

  Future<String> getValue() => _contract.read<String>('getValue');

  (String, String) setValue(String value) =>
      (_address, _interface.encodeFunctionData('setValue', [value]));
}
