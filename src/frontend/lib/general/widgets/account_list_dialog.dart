import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

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
      title: Text(
        'Wallet accounts',
        style: GoogleFonts.philosopher(
          color: Colors.white,
          fontSize: 24,
        ),
      ),
      children: [
        SizedBox(
          width: 400,
          child: StreamBuilder(
            stream: _userBloc.walletAddresses$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return const Center(
                  child: CircularProgressIndicator(),
                );
              }

              var addresses = snapshot.data!;
              return Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  ...addresses.map(
                    (address) => ListTile(
                      title: FittedBox(
                        child: SelectableText(
                          address,
                          style: GoogleFonts.righteous(
                            color: Colors.white,
                          ),
                        ),
                      ),
                      trailing: _currentWalletAddress == address
                          ? const Icon(
                              Icons.check_box_outlined,
                              color: Colors.white,
                            )
                          : InkWell(
                              child: const Icon(
                                Icons.check_box_outline_blank_outlined,
                                color: Colors.white,
                              ),
                              onTap: () async {
                                await _userBloc.execute(SwitchAccount(walletAddress: address));
                                if (context.mounted) Navigator.of(context).pop();
                              },
                            ),
                    ),
                  ),
                  Divider(color: Colors.white),
                  ListTile(
                    title: Text(
                      'Add account',
                      textAlign: TextAlign.end,
                      style: GoogleFonts.raleway(
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
                  ListTile(
                    title: Text(
                      'Reveal secret phrase',
                      textAlign: TextAlign.end,
                      style: GoogleFonts.raleway(
                        color: Colors.white,
                      ),
                    ),
                    trailing: const Icon(
                      Icons.text_snippet,
                      color: Colors.white,
                    ),
                    onTap: () => multiStageOffChainFlow(
                      context,
                      (ctx) => _userBloc.executeMultiStage(
                        const RevealSecretPhrase(),
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
