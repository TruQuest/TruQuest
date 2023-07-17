import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum_js_interop.dart';

class EthereumRpcProvider {
  late final JsonRpcProvider provider;

  EthereumRpcProvider() {
    provider = JsonRpcProvider(dotenv.env['ETHEREUM_RPC_URL']!);
  }
}
