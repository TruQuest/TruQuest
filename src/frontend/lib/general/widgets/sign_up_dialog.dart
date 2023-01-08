// ignore_for_file: prefer_const_constructors

import "package:flutter/material.dart";

import "../../user/models/vm/user_vm.dart";
import "../../user/bloc/user_result_vm.dart";
import "../../ethereum/bloc/ethereum_bloc.dart";
import "../../user/bloc/user_actions.dart";
import "../../user/bloc/user_bloc.dart";
import "../../widget_extensions.dart";
import "../../ethereum/bloc/ethereum_actions.dart";
import "../../ethereum/bloc/ethereum_result_vm.dart";

class SignUpDialog extends StatefulWidget {
  const SignUpDialog({super.key});

  @override
  State<SignUpDialog> createState() => _SignUpDialogState();
}

class _SignUpDialogState extends StateX<SignUpDialog> {
  late final _userBloc = use<UserBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  String? _username;

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text("Sign Up"),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextField(
            onChanged: (value) {
              _username = value;
            },
            decoration: InputDecoration(hintText: "Username"),
          ),
          SizedBox(height: 12),
          StreamBuilder<LoadCurrentUserSuccessVm>(
            stream: _userBloc.currentUser$,
            initialData: _userBloc.currentUser$last,
            builder: (context, snapshot) {
              var user = snapshot.data!.user;
              if (user.state == UserAccountState.guest) {
                return TextButton(
                  child: Text("Connect account"),
                  onPressed: () async {
                    var action = ConnectEthereumAccount();
                    _ethereumBloc.dispatch(action);

                    var error = await action.result;
                    if (error != null) {
                      // do smth
                    }
                  },
                );
              }

              return Text(user.ethereumAccount!);
            },
          ),
        ],
      ),
      actions: [
        TextButton(
          child: Text("Sign Up"),
          onPressed: () async {
            if (_username != null) {
              var signAction = SignAuthMessage(username: _username!);
              _ethereumBloc.dispatch(signAction);

              var vm = await signAction.result;
              if (vm is SignAuthMessageFailureVm) {
                return;
              }

              vm as SignAuthMessageSuccessVm;

              var signUpAction = SignUp(
                account: vm.account,
                username: _username!,
                signature: vm.signature,
              );
              _userBloc.dispatch(signUpAction);

              var error = await signUpAction.result;
              if (error != null) {
                return;
              }

              Navigator.of(this.context).pop();
            }
          },
        ),
      ],
    );
  }
}
