import 'package:flutter/material.dart';

import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class RestrictWhenNoWalletButton extends StatelessWidgetX {
  late final _ethereumBloc = use<EthereumBloc>();

  final Widget child;

  RestrictWhenNoWalletButton({super.key, required this.child});

  @override
  Widget buildX(BuildContext context) {
    return Stack(
      children: [
        child,
        Positioned.fill(
          child: IgnorePointer(
            ignoring: _ethereumBloc.isAvailable,
            child: GestureDetector(
              onTap: () => showDialog(
                context: context,
                builder: (context) => AlertDialog(
                  title: Text('Metamask not installed'),
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}
