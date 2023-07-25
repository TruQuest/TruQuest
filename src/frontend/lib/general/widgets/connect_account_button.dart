import 'package:flutter/material.dart';

import '../utils/utils.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';
import 'local_wallet_creation_dialog.dart';

// ignore: must_be_immutable
class ConnectAccountButton extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  ConnectAccountButton({super.key});

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        IconButton(
          icon: const Icon(
            Icons.wifi_tethering,
            color: Colors.white,
          ),
          onPressed: () {
            if (!_userBloc.localWalletSelected) {
              multiStageOffChainAction(
                context,
                (ctx) => _userBloc.connectAccount(ctx),
              );
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
                        onPressed: () => Navigator.of(context).pop('Local'),
                      ),
                      TextButton(
                        child: const Text('Metamask'),
                        onPressed: () => Navigator.of(context).pop('Metamask'),
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
                      builder: (_) => const LocalWalletCreationDialog(),
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
                  // ignore: use_build_context_synchronously
                  multiStageOffChainAction(
                    context,
                    (ctx) => _userBloc.connectAccount(ctx),
                  );
                }
              },
            ),
          ),
        ),
      ],
    );
  }
}
