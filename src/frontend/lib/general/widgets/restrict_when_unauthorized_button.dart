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
              var user = snapshot.data;
              return IgnorePointer(
                ignoring: user?.id != null && false,
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
                                  false
                                      ? const Icon(
                                          Icons.check_box,
                                          color: Colors.green,
                                        )
                                      : Text('$i)'),
                                  const SizedBox(width: 6),
                                  const Text('Select a wallet'),
                                  if (true)
                                    Padding(
                                      padding: const EdgeInsets.only(left: 6),
                                      child: WalletSelectionButton(),
                                    ),
                                ],
                              ),
                              if (i++ > 0) const SizedBox(height: 8),
                              Row(
                                children: [
                                  false
                                      ? const Icon(
                                          Icons.check_box,
                                          color: Colors.green,
                                        )
                                      : Text('$i)'),
                                  const SizedBox(width: 6),
                                  Text(
                                    'Connect the wallet to',
                                  ),
                                ],
                              ),
                              if (i++ > 0) const SizedBox(height: 8),
                              Row(
                                children: [
                                  user != null
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
