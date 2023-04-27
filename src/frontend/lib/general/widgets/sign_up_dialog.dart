import 'package:flutter/material.dart';

import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';
import '../../ethereum/bloc/ethereum_actions.dart';
import '../../ethereum/bloc/ethereum_result_vm.dart';

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
      title: Text('Sign Up'),
      content: TextField(
        onChanged: (value) => _username = value,
        decoration: InputDecoration(hintText: 'Username'),
      ),
      actions: [
        TextButton(
          child: Text('Sign Up'),
          onPressed: () async {
            if (_username != null) {
              var signAction = SignSignUpMessage(username: _username!);
              _ethereumBloc.dispatch(signAction);

              var vm = await signAction.result;
              if (vm is SignSignUpMessageFailureVm) {
                return;
              }

              vm as SignSignUpMessageSuccessVm;

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
