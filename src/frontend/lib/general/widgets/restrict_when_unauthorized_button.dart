import 'package:flutter/material.dart';

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
                ignoring: user?.id != null,
                child: GestureDetector(
                  onTap: () => showDialog(
                    context: context,
                    builder: (context) => AlertDialog(
                      title: Text('You are in read-only mode'),
                      content: SizedBox(
                        width: 500,
                        height: 200,
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Row(
                              children: [
                                _ethereumBloc.isAvailable
                                    ? Icon(
                                        Icons.check_box,
                                        color: Colors.green,
                                      )
                                    : Text('1)'),
                                SizedBox(width: 6),
                                Text('Install Metamask'),
                              ],
                            ),
                            SizedBox(height: 8),
                            Row(
                              children: [
                                user?.ethereumAccount != null
                                    ? Icon(
                                        Icons.check_box,
                                        color: Colors.green,
                                      )
                                    : Text('2)'),
                                SizedBox(width: 6),
                                Text(
                                  'Connect your Ethereum account to TruQuest',
                                ),
                              ],
                            ),
                            SizedBox(height: 8),
                            Row(
                              children: [
                                Text('3)'),
                                SizedBox(width: 6),
                                Text('Sign-in to TruQuest'),
                              ],
                            ),
                          ],
                        ),
                      ),
                      actions: [
                        TextButton(
                          child: Text('Dismiss'),
                          onPressed: () => Navigator.of(context).pop(),
                        ),
                      ],
                    ),
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
