import 'package:flutter/material.dart';
import 'package:auto_size_text/auto_size_text.dart';

import '../utils/utils.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class AccountListDialog extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  late final String? _currentWalletAddress;

  AccountListDialog({super.key}) {
    _currentWalletAddress = _userBloc.latestCurrentUser?.walletAddress;
  }

  @override
  Widget buildX(BuildContext context) {
    return SimpleDialog(
      backgroundColor: const Color(0xFF242423),
      title: const Text(
        'Wallet accounts',
        style: TextStyle(
          color: Colors.white,
        ),
      ),
      children: [
        SizedBox(
          width: 400,
          height: 400,
          child: StreamBuilder(
            stream: _userBloc.walletAddresses$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return const Center(
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
                        style: const TextStyle(
                          color: Colors.white,
                        ),
                      ),
                      trailing: _currentWalletAddress == address
                          ? const Icon(
                              Icons.check_box_outlined,
                              color: Colors.white,
                            )
                          : IconButton(
                              icon: const Icon(
                                Icons.check_box_outline_blank_outlined,
                                color: Colors.white,
                              ),
                              onPressed: () async {
                                await _userBloc.execute(SwitchAccount(walletAddress: address));
                                if (context.mounted) Navigator.of(context).pop();
                              },
                            ),
                    ),
                  ),
                  ListTile(
                    title: const Text(
                      'Add account',
                      style: TextStyle(
                        color: Colors.white,
                      ),
                    ),
                    trailing: const Icon(
                      Icons.add,
                      color: Colors.white,
                    ),
                    onTap: () => multiStageOffChainFlow(
                      context,
                      (ctx) => _userBloc.executeMultiStage(
                        const AddAccount(),
                        ctx,
                      ),
                    ),
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
