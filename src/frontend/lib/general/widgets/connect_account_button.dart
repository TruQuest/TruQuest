import 'package:flutter/material.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../../ethereum/bloc/ethereum_actions.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class ConnectAccountButton extends StatelessWidgetX {
  late final _ethereumBloc = use<EthereumBloc>();

  ConnectAccountButton({super.key});

  void _connectAccount(BuildContext context) async {
    var action = ConnectEthereumAccount();
    _ethereumBloc.dispatch(action);

    var success = await action.result;
    if (success?.walletConnectUri != null) {
      if (context.mounted) {
        showDialog(
          context: context,
          builder: (context) => AlertDialog(
            title: const Text('Scan the QR code'),
            content: Container(
              width: 500,
              height: 500,
              alignment: Alignment.center,
              child: QrImageView(
                data: success!.walletConnectUri!,
                version: QrVersions.auto,
                size: 400,
              ),
            ),
          ),
        );
      }
    }
  }

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        IconButton(
          icon: const Icon(
            Icons.wifi_tethering,
            color: Colors.white,
          ),
          onPressed: () => _connectAccount(context),
        ),
        Positioned.fill(
          child: IgnorePointer(
            ignoring: false,
            child: GestureDetector(
              onTap: () async {
                var walletName = await showDialog<String>(
                  context: context,
                  builder: (context) => AlertDialog(
                    title: const Text('Select a wallet'),
                    actions: [
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

                if (walletName != null) {
                  var action = SelectWallet(walletName: walletName);
                  _ethereumBloc.dispatch(action);

                  var failure = await action.result;
                  if (failure == null) {
                    // ignore: use_build_context_synchronously
                    _connectAccount(context);
                  }
                }
              },
            ),
          ),
        ),
      ],
    );
  }
}
