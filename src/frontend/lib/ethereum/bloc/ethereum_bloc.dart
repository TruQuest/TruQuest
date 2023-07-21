import 'dart:async';

import 'ethereum_actions.dart';
import '../services/ethereum_rpc_provider.dart';
import '../../general/bloc/bloc.dart';

class EthereumBloc extends Bloc<EthereumAction> {
  final EthereumRpcProvider _ethereumRpcProvider;

  Stream<int> get latestL1BlockNumber$ =>
      _ethereumRpcProvider.latestL1BlockNumber$;
  int get latestL1BlockNumber => _ethereumRpcProvider.latestL1BlockNumber;

  EthereumBloc(this._ethereumRpcProvider);
}
