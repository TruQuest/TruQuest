import 'dart:async';

import 'package:rxdart/rxdart.dart';

import 'user_actions.dart';
import 'user_result_vm.dart';
import '../services/user_service.dart';
import '../../general/bloc/bloc.dart';

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;

  final _currentUserChannel = BehaviorSubject<LoadCurrentUserSuccessVm>();
  Stream<LoadCurrentUserSuccessVm> get currentUser$ =>
      _currentUserChannel.stream;
  LoadCurrentUserSuccessVm? get latestCurrentUser =>
      _currentUserChannel.valueOrNull;

  UserBloc(this._userService) {
    actionChannel.stream.listen((action) {
      if (action is CreateSmartWallet) {
        _createSmartWallet(action);
      } else if (action is EncryptAndSaveSmartWallet) {
        _encryptAndSaveSmartWallet(action);
      } else if (action is SignInWithEthereum) {
        _signInWithEthereum(action);
      } else if (action is AddEmail) {
        _addEmail(action);
      } else if (action is ConfirmEmail) {
        _confirmEmail(action);
      }
    });

    _userService.currentUserChanged$.listen((user) {
      _currentUserChannel.add(LoadCurrentUserSuccessVm(user: user));
    });
  }

  void _createSmartWallet(CreateSmartWallet action) async {
    var wallet = await _userService.createSmartWallet();
    action.complete(CreateSmartWalletSuccessVm(wallet: wallet));
  }

  void _encryptAndSaveSmartWallet(EncryptAndSaveSmartWallet action) async {
    await _userService.encryptSmartWalletAndSaveToLocalStorage(
      action.wallet,
      action.password,
    );
    action.complete(EncryptAndSaveSmartWalletSuccessVm());
  }

  void _signInWithEthereum(SignInWithEthereum action) async {
    await _userService.signInWithEthereum(action.password);
    action.complete(SignInWithEthereumSuccessVm());
  }

  void _addEmail(AddEmail action) async {
    await _userService.addEmail(action.email);
    action.complete(AddEmailSuccessVm());
  }

  void _confirmEmail(ConfirmEmail action) async {
    await _userService.confirmEmail(action.confirmationToken);
    action.complete(ConfirmEmailSuccessVm());
  }
}
