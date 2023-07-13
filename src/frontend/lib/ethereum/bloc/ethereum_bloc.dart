import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../../general/contracts/truquest_contract.dart';
import '../../general/contracts/truthserum_contract.dart';
import 'ethereum_actions.dart';
import 'ethereum_result_vm.dart';
import '../services/ethereum_service.dart';
import '../../general/bloc/bloc.dart';

class EthereumBloc extends Bloc<EthereumAction> {
  final EthereumService _ethereumService;
  final TruthserumContract _truthserumContract;
  final TruQuestContract _truQuestContract;

  bool get walletSetup => _ethereumService.walletSetup.isCompleted;

  bool get connectedToValidChain =>
      _ethereumService.connectedChainId == _ethereumService.validChainId;
  String get validChainName => _ethereumService.validChainName;

  final BehaviorSubject<int> _latestL1BlockNumberChannel =
      BehaviorSubject<int>();
  Stream<int> get latestL1BlockNumber$ => _latestL1BlockNumberChannel.stream;

  EthereumBloc(
    this._ethereumService,
    this._truthserumContract,
    this._truQuestContract,
  ) {
    actionChannel.stream.listen((action) {
      if (action is SelectWallet) {
        _selectWallet(action);
      } else if (action is SwitchEthereumChain) {
        _switchEthereumChain(action);
      } else if (action is ConnectEthereumAccount) {
        _connectEthereumAccount(action);
      } else if (action is WatchTruthserum) {
        _watchTruthserum(action);
      } else if (action is ApproveFundsUsage) {
        _approveFundsUsage(action);
      } else if (action is DepositFunds) {
        _depositFunds(action);
      }
    });

    _ethereumService.l1BlockMined$.listen((blockNumber) {
      _latestL1BlockNumberChannel.add(blockNumber);
    });
  }

  void _selectWallet(SelectWallet action) async {
    await _ethereumService.selectWallet(action.walletName);
    action.complete(null);
  }

  void _switchEthereumChain(SwitchEthereumChain action) async {
    await _ethereumService.switchEthereumChain();
  }

  void _connectEthereumAccount(ConnectEthereumAccount action) async {
    var result = await _ethereumService.connectAccount();
    action.complete(
      result.isLeft
          ? null
          : ConnectEthereumAccountSuccessVm(walletConnectUri: result.right),
    );
  }

  void _watchTruthserum(WatchTruthserum action) =>
      _ethereumService.watchTruthserum();

  void _approveFundsUsage(ApproveFundsUsage action) async {
    var error = await _truthserumContract.approve(action.amount);
    action.complete(error != null ? const ApproveFundsUsageFailureVm() : null);
  }

  void _depositFunds(DepositFunds action) async {
    await _truQuestContract.depositFunds(action.amount);
  }
}
