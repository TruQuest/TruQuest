import 'dart:async';

import 'package:flutter/scheduler.dart';
import 'package:rxdart/rxdart.dart';
import 'package:universal_html/html.dart' as html;

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

  final BehaviorSubject<SwitchEthereumChainSuccessVm> _selectedChainChannel =
      BehaviorSubject<SwitchEthereumChainSuccessVm>();
  Stream<SwitchEthereumChainSuccessVm> get selectedChain$ =>
      _selectedChainChannel.stream;

  final BehaviorSubject<int> _latestL1BlockNumberChannel =
      BehaviorSubject<int>();
  Stream<int> get latestL1BlockNumber$ => _latestL1BlockNumberChannel.stream;

  EthereumBloc(
    this._ethereumService,
    this._truthserumContract,
    this._truQuestContract,
  ) {
    actionChannel.stream.listen((action) {
      if (action is SwitchEthereumChain) {
        _switchEthereumChain(action);
      } else if (action is ConnectEthereumAccount) {
        _connectEthereumAccount(action);
      } else if (action is ApproveFundsUsage) {
        _approveFundsUsage(action);
      } else if (action is DepositFunds) {
        _depositFunds(action);
      }
    });

    _ethereumService.connectedChainChanged$.listen((event) {
      var (chainId, shouldReloadPage) = event;
      if (shouldReloadPage) {
        SchedulerBinding.instance.addPostFrameCallback(
          (_) => html.window.location.reload(),
        );
      }

      _selectedChainChannel.add(SwitchEthereumChainSuccessVm(
        chainId: chainId,
        shouldOfferToSwitchChain: chainId != _ethereumService.validChainId,
      ));
    });

    _ethereumService.l1BlockMined$.listen((blockNumber) {
      _latestL1BlockNumberChannel.add(blockNumber);
    });
  }

  void _switchEthereumChain(SwitchEthereumChain action) async {
    var error = await _ethereumService.switchEthereumChain();
    action.complete(error != null ? SwitchEthereumChainFailureVm() : null);
  }

  void _connectEthereumAccount(ConnectEthereumAccount action) async {
    var error = await _ethereumService.connectAccount();
    action.complete(error != null ? ConnectEthereumAccountFailureVm() : null);
  }

  void _approveFundsUsage(ApproveFundsUsage action) async {
    var error = await _truthserumContract.approve(action.amount);
    action.complete(error != null ? ApproveFundsUsageFailureVm() : null);
  }

  void _depositFunds(DepositFunds action) async {
    await _truQuestContract.depositFunds(action.amount);
  }
}
