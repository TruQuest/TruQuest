// ignore_for_file: prefer_const_constructors

import "package:flutter/material.dart";
import "package:flutter/scheduler.dart";
import "package:universal_html/html.dart" as html;

import "../../ethereum/bloc/ethereum_bloc.dart";
import "../../ethereum/bloc/ethereum_result_vm.dart";
import "sign_up_dialog.dart";
import "../../user/bloc/user_bloc.dart";
import "../../widget_extensions.dart";
import "../../user/bloc/user_actions.dart";
import "../../user/bloc/user_result_vm.dart";
import "../../user/models/vm/user_vm.dart";

class StatusPanel extends StatelessWidgetInject2<UserBloc, EthereumBloc> {
  late final UserBloc _userBloc = service1;
  late final EthereumBloc _ethereumBloc = service2;

  StatusPanel({super.key});

  @override
  Widget buildInject(BuildContext context) {
    _userBloc.dispatchAction(LoadCurrentUser());

    return Row(
      children: [
        StreamBuilder<SwitchEthereumChainResultVm>(
          stream: _ethereumBloc.selectedChain$,
          builder: (context, snapshot) {
            if (snapshot.data == null) {
              return CircularProgressIndicator(color: Colors.white);
            }

            var vm = (snapshot.data as SwitchEthereumChainSuccessVm);
            if (vm.shouldRefreshPage) {
              SchedulerBinding.instance.addPostFrameCallback(
                (_) => html.window.location.reload(),
              );
            }

            return Text(
              "Chain Id: ${vm.chainId}",
              style: TextStyle(color: Colors.white),
            );
          },
        ),
        SizedBox(width: 64),
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
