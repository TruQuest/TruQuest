import 'dart:async';
import 'dart:convert';

import 'package:rxdart/rxdart.dart';

import '../../ethereum/errors/user_operation_error.dart';
import '../../ethereum/services/embedded_wallet_service.dart';
import '../../ethereum/services/ethereum_api_service.dart';
import '../../general/utils/utils.dart';
import '../models/vm/smart_wallet_info_vm.dart';
import '../../general/errors/insufficient_balance_error.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../../general/contracts/truthserum_contract.dart';
import '../../ethereum/services/iwallet_service.dart';
import '../../ethereum/services/third_party_wallet_service.dart';
import '../../ethereum/services/user_operation_service.dart';
import '../../general/contracts/truquest_contract.dart';
import '../models/vm/user_vm.dart';
import '../../general/services/local_storage.dart';
import '../../general/services/server_connector.dart';

class UserService {
  final EmbeddedWalletService _embeddedWalletService;
  final ThirdPartyWalletService _thirdPartyWalletService;
  final ServerConnector _serverConnector;
  final LocalStorage _localStorage;
  final UserOperationService _userOperationService;
  final TruQuestContract _truQuestContract;
  final TruthserumContract _truthserumContract;
  final EthereumApiService _ethereumApiService;

  late final IWalletService _walletService;

  final _currentUserChangedEventChannel = BehaviorSubject<UserVm>();
  Stream<UserVm> get currentUserChanged$ => _currentUserChangedEventChannel.stream;
  UserVm? get latestCurrentUser => _currentUserChangedEventChannel.valueOrNull;

  final _smartWalletInfoChannel = BehaviorSubject<SmartWalletInfoVm>();
  Stream<SmartWalletInfoVm> get smartWalletInfo$ => _smartWalletInfoChannel.stream;

  UserService(
    this._embeddedWalletService,
    this._thirdPartyWalletService,
    this._serverConnector,
    this._localStorage,
    this._userOperationService,
    this._truQuestContract,
    this._truthserumContract,
    this._ethereumApiService,
  ) {
    var walletJson = _localStorage.getString('Wallet');
    if (walletJson == null) {
      _embeddedWalletService.onSelectedForOnboarding = () {
        _walletService = _embeddedWalletService;
        _walletService.currentSignerChanged$.listen(_reloadUser);
      };
      _thirdPartyWalletService.onSelectedForOnboarding = () {
        _walletService = _thirdPartyWalletService;
        _walletService.currentSignerChanged$.listen(_reloadUser);
      };

      _reloadUser(null);
      return;
    }

    var wallet = jsonDecode(walletJson) as Map<String, dynamic>;
    if (wallet['name'] == _embeddedWalletService.name) {
      _setupEmbeddedWallet(wallet);
    } else {
      _setupThirdPartyWallet(wallet['name']);
    }
  }

  void _setupEmbeddedWallet(Map<String, dynamic> wallet) {
    _embeddedWalletService.setup(wallet);
    _walletService = _embeddedWalletService;
    _walletService.currentSignerChanged$.listen(_reloadUser);
  }

  void _setupThirdPartyWallet(String walletName) async {
    await _thirdPartyWalletService.setup(walletName);
    _walletService = _thirdPartyWalletService;
    _walletService.currentSignerChanged$.listen(_reloadUser);
  }

  void _reloadUser(String? currentSignerAddress) {
    var walletJson = _localStorage.getString('Wallet');
    var wallet = walletJson != null ? jsonDecode(walletJson) as Map<String, dynamic> : null;
    Map<String, dynamic>? userInfo;

    if (currentSignerAddress == null || (userInfo = wallet?.getValueOrNull(currentSignerAddress)) == null) {
      _serverConnector.connectToHub(null, null);
    } else {
      _serverConnector.connectToHub(
        userInfo!['userId'],
        userInfo['token'],
      );
    }

    _currentUserChangedEventChannel.add(
      UserVm(
        originWallet: wallet?['name'],
        id: userInfo?['userId'],
        signerAddress: currentSignerAddress,
        walletAddress: userInfo?['walletAddress'],
      ),
    );

    _refreshSmartWalletInfo();
  }

  Future<String> personalSign(String message) => _walletService.personalSign(message);

  Future<String> personalSignDigest(String digest) => _walletService.personalSignDigest(digest);

  void _refreshSmartWalletInfo() async {
    var signerAddress = latestCurrentUser?.signerAddress;
    var walletAddress = latestCurrentUser?.walletAddress;

    if (walletAddress == null) {
      var info = SmartWalletInfoVm.placeholder();
      _smartWalletInfoChannel.add(info);
      return;
    }

    // @@TODO: Make requests concurrently.
    String? walletCode = await _ethereumApiService.getCode(walletAddress);
    BigInt? ethBalance = await _ethereumApiService.getBalance(walletAddress);
    BigInt truBalance = await _truthserumContract.balanceOf(walletAddress);
    BigInt deposited = await _truQuestContract.balanceOf(walletAddress);
    BigInt staked = await _truQuestContract.stakedBalanceOf(walletAddress);

    var info = SmartWalletInfoVm(
      signerAddress,
      walletAddress,
      walletCode != null && walletCode != '0x',
      ethBalance,
      truBalance,
      deposited,
      staked,
    );

    _smartWalletInfoChannel.add(info);
  }

  Future<BigInt> getAvailableFundsForCurrentUser() =>
      _truQuestContract.getAvailableFunds(latestCurrentUser!.walletAddress!);

  Stream<Object> depositFunds(int amount, MultiStageOperationContext ctx) async* {
    print('**************** Deposit funds ****************');

    BigInt balance = await _truthserumContract.balanceOf(latestCurrentUser!.walletAddress!);
    if (balance < BigInt.from(amount)) {
      yield const InsufficientBalanceError();
      return;
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [
        (TruthserumContract.address, _truthserumContract.approve(amount)),
        (TruQuestContract.address, _truQuestContract.depositFunds(amount)),
      ],
      functionSignature: 'Truthserum.approve(TruQuest, ${getMinLengthAmount(BigInt.from(amount), 'TRU')} TRU)\n'
          'TruQuest.deposit(${getMinLengthAmount(BigInt.from(amount), 'TRU')} TRU)',
      description: 'Approve TruQuest to use ${getMinLengthAmount(BigInt.from(amount), 'TRU')} TRU from your wallet, '
          'and then use the approval to deposit the specified amount to TruQuest for later use. '
          'Could be withdrawn back to the wallet at any time.',
      stakeSize: BigInt.from(amount),
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) yield error;

    _refreshSmartWalletInfo();
  }

  Stream<Object> withdrawFunds(int amount, MultiStageOperationContext ctx) async* {
    print('**************** Withdraw funds ****************');

    BigInt availableFunds = await getAvailableFundsForCurrentUser();
    if (availableFunds < BigInt.from(amount)) {
      yield const InsufficientBalanceError();
      return;
    }

    yield _userOperationService.prepareOneWithRealTimeFeeUpdates(
      actions: [(TruQuestContract.address, _truQuestContract.withdrawFunds(amount))],
      functionSignature: 'TruQuest.withdraw(${getMinLengthAmount(BigInt.from(amount), 'TRU')} TRU)',
      description: 'Withdraw ${getMinLengthAmount(BigInt.from(amount), 'TRU')} TRU from TruQuest back to the wallet.',
    );

    var userOp = await ctx.approveUserOpTask.future;
    if (userOp == null) return;

    var error = await _userOperationService.send(userOp);
    if (error != null) {
      if (error.isPastOrFutureExecutionRevertError) {
        if (error.extractedFromEvent) {
          var errorDescription = _truQuestContract.parseError(error.message);
          yield UserOperationError.customContractError(errorDescription.name);
        } else {
          yield UserOperationError(code: error.code);
        }
      } else {
        yield error;
      }
    }

    _refreshSmartWalletInfo();
  }
}
