import 'iaccount_contract.dart';
import '../../../ethereum_js_interop.dart';

class SimpleAccountContract implements IAccountContract {
  static final instance = SimpleAccountContract();
  static const String _abi = '''
    [
        {
            "inputs": [
                {
                    "internalType": "address",
                    "name": "dest",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "value",
                    "type": "uint256"
                },
                {
                    "internalType": "bytes",
                    "name": "func",
                    "type": "bytes"
                }
            ],
            "name": "execute",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "inputs": [
              {
                "internalType": "address[]",
                "name": "dest",
                "type": "address[]"
              },
              {
                "internalType": "bytes[]",
                "name": "func",
                "type": "bytes[]"
              }
            ],
            "name": "executeBatch",
            "outputs": [],
            "stateMutability": "nonpayable",
            "type": "function"
        }
    ]''';

  late final Abi _interface;

  SimpleAccountContract() {
    _interface = Abi(_abi);
  }

  @override
  String execute((String, String) targetAndCallData) {
    var (target, callData) = targetAndCallData;
    return _interface.encodeFunctionData(
      'execute',
      [
        target,
        0,
        callData,
      ],
    );
  }

  @override
  String executeBatch(List<(String, String)> targetAndCallDataList) {
    return _interface.encodeFunctionData(
      'executeBatch',
      [
        targetAndCallDataList.map((e) => e.$1).toList(),
        targetAndCallDataList.map((e) => e.$2).toList(),
      ],
    );
  }
}
