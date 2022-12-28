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
  final CurrentUserLoadedVm currentUserLoadedVm;

  const SignUpDialog({
    super.key,
    required this.currentUserLoadedVm,
  });

  @override
  State<SignUpDialog> createState() => _SignUpDialogState();
}

class _SignUpDialogState
    extends StateInject2<SignUpDialog, UserBloc, EthereumBloc> {
  late final UserBloc _userBloc = service1;
  late final EthereumBloc _ethereumBloc = service2;

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
          StreamBuilder<LoadCurrentUserResultVm>(
            stream: _userBloc.currentUser$,
            initialData: widget.currentUserLoadedVm,
            builder: (context, snapshot) {
              var user = (snapshot.data! as CurrentUserLoadedVm).user;
              if (user.state == UserAccountState.guest) {
                return TextButton(
                  child: Text("Connect account"),
                  onPressed: () async {
                    var action = ConnectEthereumAccount();
                    _ethereumBloc.dispatchAction(action);

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
              _ethereumBloc.dispatchAction(signAction);

              var vm = await signAction.result;
              if (vm is SignAuthMessageFailureVm) {
                return;
              }

              var signature = (vm as SignAuthMessageSuccessVm).signature;
              var signUpAction = SignUp(
                username: _username!,
                signature: signature,
              );
              _userBloc.dispatchAction(signUpAction);

              Navigator.of(this.context).pop();
            }
          },
        ),
      ],
    );
  }
}
