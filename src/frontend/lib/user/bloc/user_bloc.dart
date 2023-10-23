import 'dart:async';

import '../../ethereum_js_interop.dart';
import '../models/vm/smart_wallet_info_vm.dart';
import '../../ethereum/services/third_party_wallet_service.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../models/vm/user_vm.dart';
import '../services/embedded_wallet_service.dart';
import 'user_actions.dart';
import '../services/user_service.dart';
import '../../general/bloc/bloc.dart';

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;
  final EmbeddedWalletService _embeddedWalletService;
  final ThirdPartyWalletService _thirdPartyWalletService;

  Stream<UserVm> get currentUser$ => _userService.currentUserChanged$;
  UserVm? get latestCurrentUser => _userService.latestCurrentUser;

  Stream<SmartWalletInfoVm> get smartWalletInfo$ => _userService.smartWalletInfo$;

  UserBloc(
    super.toastMessenger,
    this._userService,
    this._embeddedWalletService,
    this._thirdPartyWalletService,
  );

  @override
  Future<Object?> handleExecute(UserAction action) async {
    if (action is GenerateConfirmationCodeAndAttestationOptions) {
      return _generateConfirmationCodeAndAttestationOptions(action);
    } else if (action is SignUp) {
      return _signUp(action);
    }

    throw UnimplementedError();
  }

  @override
  Stream<Object> handleMultiStageExecute(UserAction action, MultiStageOperationContext ctx) {
    if (action is SignInWithThirdPartyWallet) {
      return _signInWithThirdPartyWallet(action, ctx);
    } else if (action is DepositFunds) {
      return _depositFunds(action, ctx);
    } else if (action is WithdrawFunds) {
      return _withdrawFunds(action, ctx);
    }

    throw UnimplementedError();
  }

  Future<AttestationOptions> _generateConfirmationCodeAndAttestationOptions(
    GenerateConfirmationCodeAndAttestationOptions action,
  ) {
    return _embeddedWalletService.generateConfirmationCodeAndAttestationOptions(action.email);
  }

  Future<bool> _signUp(SignUp action) =>
      _embeddedWalletService.signUp(action.email, action.confirmationCode, action.options);

  Stream<Object> _signInWithThirdPartyWallet(SignInWithThirdPartyWallet action, MultiStageOperationContext ctx) =>
      _thirdPartyWalletService.signIn(action.walletName, ctx);

  Stream<Object> _depositFunds(DepositFunds action, MultiStageOperationContext ctx) =>
      _userService.depositFunds(action.amount, ctx);

  Stream<Object> _withdrawFunds(WithdrawFunds action, MultiStageOperationContext ctx) =>
      _userService.withdrawFunds(action.amount, ctx);
}
