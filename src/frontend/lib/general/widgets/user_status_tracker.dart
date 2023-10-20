import 'package:flutter/material.dart';

import 'connect_account_button.dart';
import 'onboarding_dialog.dart';
import 'sign_in_button.dart';
import 'account_list_dialog.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class UserStatusTracker extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  UserStatusTracker({super.key});

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      builder: (context, snapshot) {
        return IconButton(
          icon: Icon(
            Icons.abc,
            color: Colors.white,
          ),
          onPressed: () => showDialog(
            context: context,
            builder: (context) => OnboardingDialog(),
          ),
        );

        if (snapshot.data == null) {
          return const SizedBox.shrink();
        }

        var user = snapshot.data!;
        if (user.walletAddress == null) {
          return Tooltip(
            message: 'Connect',
            child: ConnectAccountButton(),
          );
        } else if (user.id == null) {
          return const Tooltip(
            message: 'Sign-in',
            child: SignInButton(),
          );
        }

        return Tooltip(
          message: user.id,
          child: IconButton(
            icon: const Icon(
              Icons.account_box,
              color: Colors.white,
            ),
            onPressed: () {
              if (_userBloc.localWalletSelected) {
                showDialog(
                  context: context,
                  builder: (_) => AccountListDialog(),
                );
              }
            },
          ),
        );
      },
    );
  }
}
