import '../../ethereum_js_interop.dart';
import '../../ethereum/services/ethereum_api_service.dart';

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

  DummyContract(EthereumApiService ethereumApiService) {
    _interface = Abi(_abi);
    _contract = Contract(_address, _abi, ethereumApiService.provider);
  }

  Future<String> getValue() => _contract.read<String>('getValue');

  (String, String) setValue(String value) =>
      (_address, _interface.encodeFunctionData('setValue', [value]));
}
