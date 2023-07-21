import 'package:rxdart/rxdart.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum_js_interop.dart';

class EthereumRpcProvider {
  late final JsonRpcProvider provider;
  late final JsonRpcProvider l1Provider;

  final _latestL1BlockNumberChannel = BehaviorSubject<int>();
  Stream<int> get latestL1BlockNumber$ => _latestL1BlockNumberChannel.stream;
  int get latestL1BlockNumber => _latestL1BlockNumberChannel.value;

  EthereumRpcProvider() {
    provider = JsonRpcProvider(dotenv.env['ETHEREUM_RPC_URL']!);
    l1Provider = JsonRpcProvider(dotenv.env['ETHEREUM_L1_RPC_URL']!);
  }

  Future init() async {
    l1Provider.removeAllListeners('block');
    var blockNumber = await l1Provider.getBlockNumber();
    _latestL1BlockNumberChannel.add(blockNumber);

    l1Provider.onBlockMined((blockNumber) {
      print('Latest L1 block: $blockNumber');
      _latestL1BlockNumberChannel.add(blockNumber);
    });
  }
}
