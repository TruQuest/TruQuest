import '../../ethereum_js_interop.dart';

abstract class BaseContract {
  final String address;
  final Abi interface;
  final Contract contract;

  BaseContract(this.address, String abi, JsonRpcProvider provider)
      : interface = Abi(abi),
        contract = Contract(address, abi, provider);

  ErrorDescription parseError(String data) => interface.parseError(data);
}
