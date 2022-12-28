// ignore_for_file: prefer_const_constructors

import "package:flutter/material.dart";

import "sign_up_dialog.dart";
import "../../user/bloc/user_bloc.dart";
import "../../widget_extensions.dart";
import "../../user/bloc/user_actions.dart";
import "../../user/bloc/user_result_vm.dart";
import "../../user/models/vm/user_vm.dart";

class StatusPanel extends StatelessWidgetInject<UserBloc> {
  late final UserBloc _userBloc = service;

  StatusPanel({super.key});

  @override
  Widget buildInject(BuildContext context) {
    _userBloc.dispatchAction(LoadCurrentUser());

    return Row(
      children: [
        StreamBuilder<LoadCurrentUserResultVm>(
          stream: _userBloc.currentUser$,
          initialData: CurrentUserLoadingVm(),
          builder: (context, snapshot) {
            var vm = snapshot.data!;
            if (vm is CurrentUserLoadingVm) {
              return CircularProgressIndicator(color: Colors.white);
            }

            var user = (vm as CurrentUserLoadedVm).user;
            if (user.state == UserAccountState.guest) {
              return TextButton(
                child: Text(
                  "Sign Up",
                  style: TextStyle(color: Colors.white),
                ),
                onPressed: () {
                  showDialog(
                    context: context,
                    barrierDismissible: false,
                    builder: (_) => SignUpDialog(currentUserLoadedVm: vm),
                  );
                },
              );
            } else if (user.state == UserAccountState.connectedNotLoggedIn) {
              return TextButton(
                child: Text(
                  "Log In/Sign Up",
                  style: TextStyle(color: Colors.white),
                ),
                onPressed: () {},
              );
            }

            return Text(
              "Logged in as ${user.username}: ${user.ethereumAccount}",
              style: TextStyle(color: Colors.white),
            );
          },
        ),
      ],
    );
  }
}
