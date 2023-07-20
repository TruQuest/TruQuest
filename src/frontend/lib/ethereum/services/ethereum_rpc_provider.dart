import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum_js_interop.dart';

class EthereumRpcProvider {
  late final JsonRpcProvider provider;
  late final JsonRpcProvider l1Provider;

  EthereumRpcProvider() {
    provider = JsonRpcProvider(dotenv.env['ETHEREUM_RPC_URL']!);
    l1Provider = JsonRpcProvider(dotenv.env['ETHEREUM_L1_RPC_URL']!);
  }
}
