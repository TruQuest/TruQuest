import 'package:flutter_dotenv/flutter_dotenv.dart';

import 'entrypoint_contract.dart';
import '../../../ethereum/services/ethereum_rpc_provider.dart';

class EntryPointV060Contract extends EntryPointContract {
  late final String _address = dotenv.env['BaseGoerliEntryPointAddress']!;
  @override
  String get address => _address;

  EntryPointV060Contract(EthereumRpcProvider ethereumRpcProvider) : super(ethereumRpcProvider);
}
