import 'package:flutter/material.dart';

import 'wallet_selection_button.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class RestrictWhenNoPrimaryWalletButton extends StatelessWidgetX {
  late final _ethereumBloc = use<EthereumBloc>();

  final Widget child;

  RestrictWhenNoPrimaryWalletButton({super.key, required this.child});

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        child,
        Positioned.fill(
          child: IgnorePointer(
            ignoring: _ethereumBloc.isAvailable &&
                (!_ethereumBloc.multipleWalletsDetected ||
                    _ethereumBloc.walletSelected),
            child: GestureDetector(
              onTap: () => showDialog(
                context: context,
                builder: (context) => AlertDialog(
                  title: !_ethereumBloc.isAvailable
                      ? Text('Wallet not installed')
                      : Text('Primary wallet not selected'),
                  content: !_ethereumBloc.isAvailable
                      ? Text(
                          'Please install either Metamask or Coinbase Wallet extension',
                        )
                      : SizedBox(
                          width: 400,
                          child: Row(
                            children: [
                              Text('Select which wallet to use'),
                              SizedBox(width: 6),
                              WalletSelectionButton(),
                            ],
                          ),
                        ),
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}
