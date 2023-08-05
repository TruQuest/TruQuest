import 'dart:async';

import '../models/vm/smart_wallet_info_vm.dart';
import '../../ethereum/services/third_party_wallet_service.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../../ethereum/services/local_wallet_service.dart';
import '../models/vm/user_vm.dart';
import 'user_actions.dart';
import '../services/user_service.dart';
import '../../general/bloc/bloc.dart';

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;
  final LocalWalletService _localWalletService;
  final ThirdPartyWalletService _thirdPartyWalletService;

  Stream<UserVm> get currentUser$ => _userService.currentUserChanged$;
  UserVm? get latestCurrentUser => _userService.latestCurrentUser;

  Stream<List<String>> get walletAddresses$ => _localWalletService.walletAddresses$;

  bool get walletSelected => _userService.selectedWalletName != null;
  bool get localWalletSelected => _userService.selectedWalletName == 'Local';

  Stream<SmartWalletInfoVm> get smartWalletInfo$ => _userService.smartWalletInfo$;

  UserBloc(
    super.toastMessenger,
    this._userService,
    this._localWalletService,
    this._thirdPartyWalletService,
  ) {
    actionChannel.stream.listen((action) {
      if (action is AddEmail) {
        _addEmail(action);
      } else if (action is ConfirmEmail) {
        _confirmEmail(action);
      }
    });
  }

  @override
  Future<Object?> handleExecute(UserAction action) async {
    if (action is GenerateMnemonic) {
      return _generateMnemonic(action);
    } else if (action is CreateAndSaveEncryptedLocalWallet) {
      return _createAndSaveEncryptedLocalWallet(action);
    } else if (action is SelectThirdPartyWallet) {
      return _selectThirdPartyWallet(action);
    } else if (action is SwitchAccount) {
      return _switchAccount(action);
    } else if (action is UnlockWallet) {
      return _unlockWallet(action);
    }

    throw UnimplementedError();
  }

  @override
  Stream<Object> handleMultiStageExecute(
    UserAction action,
    MultiStageOperationContext ctx,
  ) {
    if (action is ConnectAccount) {
      return _connectAccount(action, ctx);
    } else if (action is SignInWithEthereum) {
      return _signInWithEthereum(action, ctx);
    } else if (action is AddAccount) {
      return _addAccount(action, ctx);
    } else if (action is DepositFunds) {
      return _depositFunds(action, ctx);
    }

    throw UnimplementedError();
  }

  Future<bool> _selectThirdPartyWallet(SelectThirdPartyWallet action) =>
      _userService.selectThirdPartyWallet(action.walletName);

  Stream<Object> _connectAccount(
    ConnectAccount action,
    MultiStageOperationContext ctx,
  ) =>
      _thirdPartyWalletService.connectAccount(ctx);

  Future<String> _generateMnemonic(GenerateMnemonic action) {
    var mnemonic = _localWalletService.generateMnemonic();
    return Future.value(mnemonic);
  }

  Future<bool> _createAndSaveEncryptedLocalWallet(
    CreateAndSaveEncryptedLocalWallet action,
  ) async {
    await _userService.createAndSaveEncryptedLocalWallet(
      action.mnemonic,
      action.password,
    );
    return true;
  }

  Stream<Object> _signInWithEthereum(
    SignInWithEthereum action,
    MultiStageOperationContext ctx,
  ) =>
      _userService.signInWithEthereum(ctx);

  void _addEmail(AddEmail action) async {
    await _userService.addEmail(action.email);
  }

  void _confirmEmail(ConfirmEmail action) async {
    await _userService.confirmEmail(action.confirmationToken);
  }

  Future<bool> _unlockWallet(UnlockWallet action) async {
    await _localWalletService.unlockWallet(action.password);
    return true; // @@!!
  }

  Stream<Object> _addAccount(
    AddAccount action,
    MultiStageOperationContext ctx,
  ) =>
      _localWalletService.addAccount(ctx);

  Future _switchAccount(SwitchAccount action) => _localWalletService.switchAccount(action.walletAddress);

  Stream<Object> _depositFunds(
    DepositFunds action,
    MultiStageOperationContext ctx,
  ) =>
      _userService.depositFunds(action.amount, ctx);
}
