import 'package:flutter/material.dart';

import '../utils/utils.dart';
import '../../user/errors/wallet_locked_error.dart';
import 'local_wallet_creation_dialog.dart';
import '../../user/bloc/user_actions.dart';
import 'account_list_dialog.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class UserStatusTracker extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  UserStatusTracker({super.key});

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SizedBox.shrink();
        }

        var user = snapshot.data!;
        if (user.walletAddress == null) {
          return Tooltip(
            message: 'Connect',
            child: Stack(
              children: [
                IconButton(
                  icon: const Icon(
                    Icons.wifi_tethering,
                    color: Colors.white,
                  ),
                  onPressed: () {
                    if (!_userBloc.localWalletSelected) {
                      _userBloc.dispatch(ConnectAccount());
                    }
                  },
                ),
                Positioned.fill(
                  child: IgnorePointer(
                    ignoring: _userBloc.walletSelected,
                    child: GestureDetector(
                      onTap: () async {
                        var walletName = await showDialog<String>(
                          context: context,
                          builder: (context) => AlertDialog(
                            title: const Text('Select a wallet'),
                            actions: [
                              TextButton(
                                child: const Text('Local'),
                                onPressed: () =>
                                    Navigator.of(context).pop('Local'),
                              ),
                              TextButton(
                                child: const Text('Metamask'),
                                onPressed: () =>
                                    Navigator.of(context).pop('Metamask'),
                              ),
                              TextButton(
                                child: const Text('Coinbase Wallet'),
                                onPressed: () =>
                                    Navigator.of(context).pop('CoinbaseWallet'),
                              ),
                              TextButton(
                                child: const Text('WalletConnect'),
                                onPressed: () =>
                                    Navigator.of(context).pop('WalletConnect'),
                              ),
                            ],
                          ),
                        );

                        if (walletName == null) return;

                        if (walletName == 'Local') {
                          if (context.mounted) {
                            showDialog(
                              context: context,
                              builder: (_) => LocalWalletCreationDialog(),
                            );
                          }

                          return;
                        }

                        var action = SelectThirdPartyWallet(
                          walletName: walletName,
                        );
                        _userBloc.dispatch(action);

                        var success = await action.result;
                        if (success.shouldRequestAccounts) {
                          _userBloc.dispatch(ConnectAccount());
                        }
                      },
                    ),
                  ),
                ),
              ],
            ),
          );
        } else if (user.id == null) {
          return Tooltip(
            message: 'Sign-in',
            child: IconButton(
              icon: const Icon(
                Icons.door_sliding,
                color: Colors.white,
              ),
              onPressed: () async {
                var action = SignInWithEthereum();
                _userBloc.dispatch(action);

                var failure = await action.result;
                if (failure != null && failure.error is WalletLockedError) {
                  if (context.mounted) {
                    var unlocked = await showUnlockWalletDialog(context);
                    if (unlocked) {
                      _userBloc.dispatch(action);
                      // failure = await action.result;
                    }
                  }
                }
              },
            ),
          );
        }

        return Tooltip(
          message: user.id,
          child: IconButton(
            icon: const Icon(
              Icons.account_box,
              color: Colors.white,
            ),
            onPressed: () {
              if (_userBloc.localWalletSelected) {
                showDialog(
                  context: context,
                  builder: (_) => AccountListDialog(),
                );
              }
            },
          ),
        );
      },
    );
  }
}
