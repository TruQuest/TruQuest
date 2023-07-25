import 'dart:async';

import '../../ethereum/services/third_party_wallet_service.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../errors/wallet_locked_error.dart';
import '../../ethereum/services/local_wallet_service.dart';
import '../models/vm/user_vm.dart';
import 'user_actions.dart';
import 'user_result_vm.dart';
import '../services/user_service.dart';
import '../../general/bloc/bloc.dart';

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;
  final LocalWalletService _localWalletService;
  final ThirdPartyWalletService _thirdPartyWalletService;

  Stream<UserVm> get currentUser$ => _userService.currentUserChanged$;
  UserVm? get latestCurrentUser => _userService.latestCurrentUser;

  Stream<List<String>> get walletAddresses$ =>
      _localWalletService.walletAddresses$;

  bool get walletSelected => _userService.selectedWalletName != null;
  bool get localWalletSelected => _userService.selectedWalletName == 'Local';

  UserBloc(
    super._toastMessenger,
    this._userService,
    this._localWalletService,
    this._thirdPartyWalletService,
  ) {
    actionChannel.stream.listen((action) {
      if (action is SelectThirdPartyWallet) {
        _selectThirdPartyWallet(action);
      } else if (action is GenerateMnemonic) {
        _generateMnemonic(action);
      } else if (action is CreateAndSaveEncryptedLocalWallet) {
        _createAndSaveEncryptedLocalWallet(action);
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
  }

  @override
  Stream<Object> handleMultiStage(
    UserAction action,
    MultiStageOperationContext ctx,
  ) {
    if (action is ConnectAccount) {
      return _connectAccount(action, ctx);
    } else if (action is DepositFunds) {
      return _depositFunds(action, ctx);
    }

    throw UnimplementedError();
  }

  void _selectThirdPartyWallet(SelectThirdPartyWallet action) async {
    bool shouldRequestAccounts = await _userService.selectThirdPartyWallet(
      action.walletName,
    );
    action.complete(
      SelectThirdPartyWalletSuccessVm(
        shouldRequestAccounts: shouldRequestAccounts,
      ),
    );
  }

  Stream<Object> _connectAccount(
    ConnectAccount action,
    MultiStageOperationContext ctx,
  ) async* {
    yield* _thirdPartyWalletService.connectAccount(ctx);
  }

  void _generateMnemonic(GenerateMnemonic action) async {
    var mnemonic = _localWalletService.generateMnemonic();
    action.complete(GenerateMnemonicSuccessVm(mnemonic: mnemonic));
  }

  void _createAndSaveEncryptedLocalWallet(
    CreateAndSaveEncryptedLocalWallet action,
  ) async {
    await _userService.createAndSaveEncryptedLocalWallet(
      action.mnemonic,
      action.password,
    );
    action.complete(CreateAndSaveEncryptedLocalWalletSuccessVm());
  }

  Stream<Object> signInWithEthereum(MultiStageOperationContext ctx) async* {
    yield* _userService.signInWithEthereum(ctx);
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
    await _localWalletService.unlockWallet(action.password);
    action.complete(UnlockWalletSuccessVm());
  }

  void _addAccount(AddAccount action) async {
    try {
      await _localWalletService.addAccount();
      action.complete(null);
    } on WalletLockedError catch (error) {
      action.complete(AddAccountFailureVm(error: error));
    }
  }

  void _switchAccount(SwitchAccount action) async {
    await _localWalletService.switchAccount(action.walletAddress);
    action.complete(SwitchAccountSuccessVm());
  }

  Stream<Object> _depositFunds(
    DepositFunds action,
    MultiStageOperationContext ctx,
  ) async* {
    yield* _userService.depositFunds(action.amount, ctx);
  }
}
