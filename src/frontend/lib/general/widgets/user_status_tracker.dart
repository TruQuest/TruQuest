import 'package:flutter/material.dart';

import 'account_list_dialog.dart';
import 'sign_in_dialog.dart';
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
      initialData: _userBloc.latestCurrentUser,
      builder: (context, snapshot) {
        if (snapshot.data == null) {
          return const SizedBox.shrink();
        }

        var user = snapshot.data!.user;
        if (user.id == null) {
          return Tooltip(
            message: 'Sign-in',
            child: IconButton(
              icon: const Icon(
                Icons.wifi_tethering,
                color: Colors.white,
              ),
              onPressed: () => showDialog(
                context: context,
                builder: (_) => SignInDialog(),
              ),
            ),
          );
        }

        return Tooltip(
          message: user.id,
          child: IconButton(
            icon: const Icon(
              Icons.account_box,
              color: Colors.white,
            ),
            onPressed: () => showDialog(
              context: context,
              builder: (_) => AccountListDialog(),
            ),
          ),
        );
      },
    );
  }
}
