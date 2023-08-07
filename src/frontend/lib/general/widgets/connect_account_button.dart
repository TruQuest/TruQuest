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
              multiStageOffChainFlow(
                context,
                (ctx) => _userBloc.executeMultiStage(
                  const ConnectAccount(),
                  ctx,
                ),
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
                        onPressed: () => Navigator.of(context).pop('CoinbaseWallet'),
                      ),
                      TextButton(
                        child: const Text('WalletConnect'),
                        onPressed: () => Navigator.of(context).pop('WalletConnect'),
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

                var shouldRequestAccounts = await _userBloc.execute<bool>(
                  SelectThirdPartyWallet(walletName: walletName),
                );

                if (shouldRequestAccounts.isTrue) {
                  // ignore: use_build_context_synchronously
                  multiStageOffChainFlow(
                    context,
                    (ctx) => _userBloc.executeMultiStage(
                      const ConnectAccount(),
                      ctx,
                    ),
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