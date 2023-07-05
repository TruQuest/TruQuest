import 'package:flutter/material.dart';

import '../../ethereum/bloc/ethereum_actions.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class WalletSelectionButton extends StatelessWidgetX {
  late final _ethereumBloc = use<EthereumBloc>();

  WalletSelectionButton({super.key});

  @override
  Widget buildX(BuildContext context) {
    return IconButton(
      icon: Icon(
        Icons.wallet,
        color: Colors.black,
      ),
      onPressed: () async {
        var walletName = await showDialog<String>(
          context: context,
          builder: (context) => AlertDialog(
            title: Text('Choose wallet'),
            actions: [
              TextButton(
                child: Text('Metamask'),
                onPressed: () => Navigator.of(context).pop('Metamask'),
              ),
              TextButton(
                child: Text('Coinbase Wallet'),
                onPressed: () => Navigator.of(context).pop('CoinbaseWallet'),
              ),
            ],
          ),
        );

        if (walletName != null) {
          _ethereumBloc.dispatch(SelectWallet(walletName: walletName));
          if (context.mounted) {
            Navigator.of(context).pop();
          }
        }
      },
    );
  }
}
