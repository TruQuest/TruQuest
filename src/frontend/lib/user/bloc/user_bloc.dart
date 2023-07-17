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

  Stream<List<String>> get walletAddresses$ => _userService.walletAddresses$;

  UserBloc(this._userService) {
    actionChannel.stream.listen((action) {
      if (action is CreateSmartWallet) {
        _createSmartWallet(action);
      } else if (action is CreateSmartWalletFromMnemonic) {
        _createSmartWalletFromMnemonic(action);
      } else if (action is EncryptAndSaveSmartWallet) {
        _encryptAndSaveSmartWallet(action);
      } else if (action is SignInWithEthereum) {
        _signInWithEthereum(action);
      } else if (action is AddEmail) {
        _addEmail(action);
      } else if (action is ConfirmEmail) {
        _confirmEmail(action);
      } else if (action is UnlockWallet) {
        _unlockWallet(action);
      } else if (action is AddAccount) {
        _addAccount(action);
      } else if (action is SwitchAccount) {
        _switchAccount(action);
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

  void _createSmartWalletFromMnemonic(
    CreateSmartWalletFromMnemonic action,
  ) async {
    var wallet = await _userService.createSmartWalletFromMnemonic(
      action.mnemonic,
    );
    action.complete(CreateSmartWalletFromMnemonicSuccessVm(wallet: wallet));
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

  void _unlockWallet(UnlockWallet action) async {
    await _userService.unlockWallet(action.password);
    action.complete(UnlockWalletSuccessVm());
  }

  void _addAccount(AddAccount action) async {
    var error = await _userService.addAccount();
    action.complete(error != null ? AddAccountFailureVm(error: error) : null);
  }

  void _switchAccount(SwitchAccount action) async {
    await _userService.switchAccount(action.walletAddress);
    action.complete(SwitchAccountSuccessVm());
  }
}
