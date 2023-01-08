import 'dart:async';

import 'package:flutter/scheduler.dart';
import 'package:rxdart/rxdart.dart';
import 'package:universal_html/html.dart' as html;

import 'ethereum_actions.dart';
import 'ethereum_result_vm.dart';
import '../services/ethereum_service.dart';
import '../../general/bloc/bloc.dart';

class EthereumBloc extends Bloc<EthereumAction> {
  final EthereumService _ethereumService;

  final BehaviorSubject<SwitchEthereumChainSuccessVm> _selectedChainChannel =
      BehaviorSubject<SwitchEthereumChainSuccessVm>();
  Stream<SwitchEthereumChainSuccessVm> get selectedChain$ =>
      _selectedChainChannel.stream;

  EthereumBloc(this._ethereumService) {
    actionChannel.stream.listen((action) {
      if (action is SwitchEthereumChain) {
        _switchEthereumChain(action);
      } else if (action is ConnectEthereumAccount) {
        _connectEthereumAccount(action);
      } else if (action is SignAuthMessage) {
        _signAuthMessage(action);
      }
    });

    var first = true;
    _ethereumService.connectedChainChanged$.listen((chainId) {
      if (!first) {
        SchedulerBinding.instance.addPostFrameCallback(
          (_) => html.window.location.reload(),
        );
      }
      first = false;

      _selectedChainChannel.add(SwitchEthereumChainSuccessVm(
        chainId: chainId,
        shouldOfferToSwitchChain: chainId != _ethereumService.validChainId,
      ));
    });
  }

  @override
  void dispose({EthereumAction? cleanupAction}) {}

  void _switchEthereumChain(SwitchEthereumChain action) async {
    var error = await _ethereumService.switchEthereumChain();
    action.complete(error != null ? SwitchEthereumChainFailureVm() : null);
  }

  void _connectEthereumAccount(ConnectEthereumAccount action) async {
    var error = await _ethereumService.connectAccount();
    action.complete(error != null ? ConnectEthereumAccountFailureVm() : null);
  }

  void _signAuthMessage(SignAuthMessage action) async {
    var result = await _ethereumService.signAuthMessage(action.username);
    if (result.isLeft) {
      action.complete(SignAuthMessageFailureVm());
      return;
    }

    action.complete(SignAuthMessageSuccessVm(
      account: result.right.item1,
      signature: result.right.item2,
    ));
  }
}
