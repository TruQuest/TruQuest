import 'package:flutter/material.dart';

import 'restrict_when_not_on_valid_chain_button.dart';
import 'connect_account_button.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../user/models/vm/user_vm.dart';
import '../../widget_extensions.dart';

// ignore: must_be_immutable
class UserStatusTracker extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();

  UserStatusTracker({super.key});

  @override
  Widget buildX(BuildContext context) {
    return StreamBuilder(
      stream: _userBloc.currentUser$,
      initialData: _userBloc.latestCurrentUser,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SizedBox.shrink();
        }

        var user = snapshot.data!.user;
        if (user.state == UserAccountState.guest) {
          return Tooltip(
            message: 'Connect',
            child: ConnectAccountButton(),
          );
        } else if (user.state == UserAccountState.connectedNotLoggedIn) {
          return Tooltip(
            message: 'Sign-in with Ethereum',
            child: RestrictWhenNotOnValidChainButton(
              child: IconButton(
                icon: const Icon(
                  Icons.door_sliding,
                  color: Colors.white,
                ),
                onPressed: () => _userBloc.dispatch(const SignInWithEthereum()),
              ),
            ),
          );
        }

        return Tooltip(
          message: user.ethereumAccount!,
          child: IconButton(
            icon: const Icon(
              Icons.account_box,
              color: Colors.white,
            ),
            onPressed: () {},
          ),
        );
      },
    );
  }
}
