// ignore_for_file: prefer_const_constructors

import "package:flutter/material.dart";

import '../../ethereum/bloc/ethereum_actions.dart';
import "../../ethereum/bloc/ethereum_bloc.dart";
import "../../ethereum/bloc/ethereum_result_vm.dart";
import "sign_up_dialog.dart";
import "../../user/bloc/user_bloc.dart";
import "../../widget_extensions.dart";
import "../../user/bloc/user_result_vm.dart";
import "../../user/models/vm/user_vm.dart";

class StatusPanel extends StatelessWidgetX {
  late final _userBloc = use<UserBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  StatusPanel({super.key});

  @override
  Widget buildX(BuildContext context) {
    return LimitedBox(
      maxWidth: 400,
      child: Row(
        children: [
          Expanded(
            child: StreamBuilder<SwitchEthereumChainSuccessVm>(
              stream: _ethereumBloc.selectedChain$,
              builder: (context, snapshot) {
                if (snapshot.data == null) {
                  return Center(
                    child: CircularProgressIndicator(color: Colors.white),
                  );
                }

                var vm = snapshot.data!;
                if (vm.shouldOfferToSwitchChain) {
                  return Row(
                    children: [
                      Text(
                        "You are on an unsupported chain",
                        style: TextStyle(color: Colors.white),
                      ),
                      SizedBox(width: 12),
                      IconButton(
                        icon: Icon(Icons.change_circle_outlined),
                        color: Colors.white,
                        onPressed: () async {
                          var action = SwitchEthereumChain();
                          _ethereumBloc.dispatch(action);

                          var error = await action.result;
                          if (error != null) {
                            // ...
                          }
                        },
                      ),
                    ],
                  );
                }

                return Text(
                  "Chain Id: ${vm.chainId}",
                  style: TextStyle(color: Colors.white),
                );
              },
            ),
          ),
          SizedBox(width: 64),
          StreamBuilder<LoadCurrentUserSuccessVm>(
            stream: _userBloc.currentUser$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return CircularProgressIndicator(color: Colors.white);
              }

              var user = snapshot.data!.user;
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
                      builder: (_) => SignUpDialog(),
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
      ),
    );
  }
}
