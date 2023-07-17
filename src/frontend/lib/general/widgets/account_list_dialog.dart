import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';

import 'unlock_wallet_dialog.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class AccountListDialog extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  late final String? _currentWalletAddress;

  AccountListDialog({super.key}) {
    _currentWalletAddress = _userBloc.latestCurrentUser?.user.walletAddress;
  }

  @override
  Widget buildX(BuildContext context) {
    return SimpleDialog(
      backgroundColor: const Color(0xFF242423),
      title: Text(
        'Wallet accounts',
        style: TextStyle(
          color: Colors.white,
        ),
      ),
      children: [
        SizedBox(
          width: 300,
          height: 400,
          child: StreamBuilder(
            stream: _userBloc.walletAddresses$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return Center(
                  child: CircularProgressIndicator(),
                );
              }

              var addresses = snapshot.data!;
              return ListView(
                children: [
                  ...addresses.map(
                    (address) => ListTile(
                      title: AutoSizeText(
                        address,
                        style: TextStyle(
                          color: Colors.white,
                        ),
                      ),
                      trailing: _currentWalletAddress == address
                          ? Icon(
                              Icons.check_box_outlined,
                              color: Colors.white,
                            )
                          : IconButton(
                              icon: Icon(
                                Icons.check_box_outline_blank_outlined,
                                color: Colors.white,
                              ),
                              onPressed: () async {
                                var action = SwitchAccount(
                                  walletAddress: address,
                                );
                                _userBloc.dispatch(action);

                                await action.result;
                                if (context.mounted) {
                                  Navigator.of(context).pop();
                                }
                              },
                            ),
                    ),
                  ),
                  ListTile(
                    title: Text(
                      'Add account',
                      style: TextStyle(
                        color: Colors.white,
                      ),
                    ),
                    trailing: Icon(
                      Icons.add,
                      color: Colors.white,
                    ),
                    onTap: () async {
                      var action = AddAccount();
                      _userBloc.dispatch(action);

                      var failure = await action.result;
                      if (failure != null &&
                          failure.error is WalletLockedError) {
                        if (context.mounted) {
                          var unlocked = await showDialog<bool>(
                            context: context,
                            barrierDismissible: false,
                            builder: (_) => UnlockWalletDialog(),
                          );

                          if (unlocked != null && unlocked) {
                            action = AddAccount();
                            _userBloc.dispatch(action);

                            failure = await action.result;
                          }
                        }
                      }
                    },
                  ),
                ],
              );
            },
          ),
        ),
      ],
    );
  }
}
