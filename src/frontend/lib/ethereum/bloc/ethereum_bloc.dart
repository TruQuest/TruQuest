import 'dart:async';

import 'package:rxdart/rxdart.dart';

import 'ethereum_actions.dart';
import 'ethereum_result_vm.dart';
import '../../general/bloc/bloc.dart';

class EthereumBloc extends Bloc<EthereumAction> {
  bool get walletSetup => false;

  bool get connectedToValidChain => false;
  String get validChainName => '';

  final BehaviorSubject<int> _latestL1BlockNumberChannel =
      BehaviorSubject<int>();
  Stream<int> get latestL1BlockNumber$ => _latestL1BlockNumberChannel.stream;

  EthereumBloc() {
    actionChannel.stream.listen((action) {
      if (action is SelectWallet) {
        _selectWallet(action);
      } else if (action is SwitchEthereumChain) {
        _switchEthereumChain(action);
      } else if (action is ConnectEthereumAccount) {
        _connectEthereumAccount(action);
      } else if (action is WatchTruthserum) {
        _watchTruthserum(action);
      }
    });

    // _ethereumService.l1BlockMined$.listen((blockNumber) {
    //   _latestL1BlockNumberChannel.add(blockNumber);
    // });
  }

  void _selectWallet(SelectWallet action) async {
    // await _ethereumService.selectWallet(action.walletName);
    // action.complete(null);
  }

  void _switchEthereumChain(SwitchEthereumChain action) async {
    // await _ethereumService.switchEthereumChain();
  }

  void _connectEthereumAccount(ConnectEthereumAccount action) async {
    // var result = await _ethereumService.connectAccount();
    // action.complete(
    //   result.isLeft
    //       ? null
    //       : ConnectEthereumAccountSuccessVm(walletConnectUri: result.right),
    // );
  }

  void _watchTruthserum(WatchTruthserum action) {
    // _ethereumService.watchTruthserum();
  }
}
