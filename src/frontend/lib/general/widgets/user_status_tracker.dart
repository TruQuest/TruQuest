import 'package:flutter/material.dart';

import 'sign_in_button.dart';
import 'qr_code_dialog.dart';
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
                  onPressed: () async {
                    if (_userBloc.localWalletSelected) return;

                    var action = ConnectAccount();
                    _userBloc.dispatch(action);

                    var success = await action.result;
                    if (success?.walletConnectUri != null) {
                      if (context.mounted) {
                        showDialog(
                          context: context,
                          builder: (_) => QrCodeDialog(
                            uri: success!.walletConnectUri!,
                          ),
                        );
                      }
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
                          var action = ConnectAccount();
                          _userBloc.dispatch(action);

                          var success = await action.result;
                          if (success?.walletConnectUri != null) {
                            if (context.mounted) {
                              showDialog(
                                context: context,
                                builder: (_) => QrCodeDialog(
                                  uri: success!.walletConnectUri!,
                                ),
                              );
                            }
                          }
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
            child: SignInButton(),
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
