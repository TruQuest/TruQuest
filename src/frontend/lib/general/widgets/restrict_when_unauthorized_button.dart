import 'package:flutter/material.dart';

import 'wallet_selection_button.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class RestrictWhenUnauthorizedButton extends StatelessWidgetX {
  late final _ethereumBloc = use<EthereumBloc>();
  late final _userBloc = use<UserBloc>();

  final Widget child;

  RestrictWhenUnauthorizedButton({super.key, required this.child});

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        child,
        Positioned.fill(
          child: StreamBuilder(
            stream: _userBloc.currentUser$,
            builder: (context, snapshot) {
              var user = snapshot.data?.user;

              return IgnorePointer(
                ignoring:
                    user?.id != null && _ethereumBloc.connectedToValidChain,
                child: GestureDetector(
                  onTap: () => showDialog(
                    context: context,
                    builder: (context) {
                      int i = 1;
                      return AlertDialog(
                        title: const Text('You are in read-only mode'),
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
                                          padding:
                                              const EdgeInsets.only(left: 6),
                                          child: WalletSelectionButton(),
                                        ),
                                    ],
                                  ),
                                ),
                              if (i++ > 0) const SizedBox(height: 8),
                              Row(
                                children: [
                                  _ethereumBloc.connectedToValidChain
                                      ? const Icon(
                                          Icons.check_box,
                                          color: Colors.green,
                                        )
                                      : Text('$i)'),
                                  const SizedBox(width: 6),
                                  Text(
                                    'Connect the wallet to ${_ethereumBloc.validChainName}',
                                  ),
                                ],
                              ),
                              if (i++ > 0) const SizedBox(height: 8),
                              Row(
                                children: [
                                  user?.ethereumAccount != null
                                      ? const Icon(
                                          Icons.check_box,
                                          color: Colors.green,
                                        )
                                      : Text('$i)'),
                                  const SizedBox(width: 6),
                                  const Text(
                                    'Connect your Ethereum account to TruQuest',
                                  ),
                                ],
                              ),
                              if (i++ > 0) const SizedBox(height: 8),
                              Row(
                                children: [
                                  user?.id != null
                                      ? const Icon(
                                          Icons.check_box,
                                          color: Colors.green,
                                        )
                                      : Text('$i)'),
                                  const SizedBox(width: 6),
                                  const Text('Sign-in to TruQuest'),
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
              );
            },
          ),
        ),
      ],
    );
  }
}
