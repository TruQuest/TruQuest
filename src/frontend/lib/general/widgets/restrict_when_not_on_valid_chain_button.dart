import 'package:flutter/material.dart';

import 'wallet_selection_button.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class RestrictWhenNotOnValidChainButton extends StatelessWidgetX {
  late final _ethereumBloc = use<EthereumBloc>();

  final Widget child;

  RestrictWhenNotOnValidChainButton({super.key, required this.child});

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        child,
        Positioned.fill(
          child: IgnorePointer(
            ignoring: _ethereumBloc.connectedToValidChain,
            child: GestureDetector(
              onTap: () => showDialog(
                context: context,
                builder: (context) {
                  int i = 1;
                  return AlertDialog(
                    title: Text(
                      'You are not connected to ${_ethereumBloc.validChainName}',
                    ),
                    content: SizedBox(
                      width: 500,
                      height: 200,
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Row(
                            children: [
                              _ethereumBloc.isAvailable
                                  ? const Icon(
                                      Icons.check_box,
                                      color: Colors.green,
                                    )
                                  : Text('$i)'),
                              const SizedBox(width: 6),
                              const Text(
                                'Install Metamask or Coinbase Wallet extension',
                              ),
                            ],
                          ),
                          if (_ethereumBloc.multipleWalletsDetected)
                            Padding(
                              padding: const EdgeInsets.only(top: 8),
                              child: Row(
                                children: [
                                  i++ > 0 && _ethereumBloc.walletSelected
                                      ? const Icon(
                                          Icons.check_box,
                                          color: Colors.green,
                                        )
                                      : Text('$i)'),
                                  const SizedBox(width: 6),
                                  const Text('Select which wallet to use'),
                                  if (!_ethereumBloc.walletSelected)
                                    Padding(
                                      padding: const EdgeInsets.only(left: 6),
                                      child: WalletSelectionButton(),
                                    ),
                                ],
                              ),
                            ),
                          const SizedBox(height: 8),
                          Row(
                            children: [
                              Text('${++i})'),
                              const SizedBox(width: 6),
                              Text(
                                'Connect the wallet to ${_ethereumBloc.validChainName}',
                              ),
                            ],
                          ),
                        ],
                      ),
                    ),
                    actions: [
                      TextButton(
                        child: const Text('Dismiss'),
                        onPressed: () => Navigator.of(context).pop(),
                      ),
                    ],
                  );
                },
              ),
            ),
          ),
        ),
      ],
    );
  }
}
