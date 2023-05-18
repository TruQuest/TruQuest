import 'package:flutter/material.dart';

import '../../user/bloc/user_result_vm.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';
import '../../ethereum/bloc/ethereum_actions.dart';
import '../../ethereum/bloc/ethereum_result_vm.dart';

enum _Mode {
  actionSelection,
  signUp,
}

class AuthDialog extends StatefulWidget {
  const AuthDialog({super.key});

  @override
  State<AuthDialog> createState() => _AuthDialogState();
}

class _AuthDialogState extends StateX<AuthDialog> {
  late final _userBloc = use<UserBloc>();
  late final _ethereumBloc = use<EthereumBloc>();

  _Mode _mode = _Mode.actionSelection;

  String? _username;

  @override
  Widget build(BuildContext context) {
    return SimpleDialog(
      backgroundColor: Colors.black,
      title: Text(
        _mode == _Mode.actionSelection ? 'Welcome to TruQuest' : 'Sign Up',
        style: TextStyle(
          color: Colors.white,
        ),
      ),
      contentPadding: const EdgeInsets.fromLTRB(12, 12, 12, 12),
      children: _mode == _Mode.actionSelection
          ? [
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.white,
                  foregroundColor: Colors.black,
                  shadowColor: Colors.white,
                ),
                child: Text('Log In'),
                onPressed: () async {
                  var action = SignIn();
                  _userBloc.dispatch(action);
                  SignInFailureVm? failure = await action.result;
                  if (failure == null) {
                    Navigator.of(this.context).pop();
                  }
                },
              ),
              SizedBox(height: 12),
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.white,
                  foregroundColor: Colors.black,
                  shadowColor: Colors.white,
                ),
                child: Text('Sign Up'),
                onPressed: () => setState(() => _mode = _Mode.signUp),
              ),
            ]
          : [
              SizedBox(
                width: 200,
                child: TextField(
                  onChanged: (value) => _username = value,
                  style: TextStyle(color: Colors.white),
                  decoration: InputDecoration(
                    hintText: 'Username',
                    hintStyle: TextStyle(color: Colors.white70),
                    enabledBorder: OutlineInputBorder(
                      borderSide: BorderSide(color: Colors.white),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderSide: BorderSide(color: Colors.white),
                    ),
                  ),
                ),
              ),
              SizedBox(height: 12),
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.white,
                  foregroundColor: Colors.black,
                  shadowColor: Colors.white,
                ),
                child: Text('Ok'),
                onPressed: () async {
                  if (_username != null) {
                    // @@TODO: Make it one action.
                    var signAction = SignSignUpMessage(username: _username!);
                    _ethereumBloc.dispatch(signAction);

                    var result = await signAction.result;
                    if (result is SignSignUpMessageFailureVm) {
                      return;
                    }

                    result as SignSignUpMessageSuccessVm;

                    var signUpAction = SignUp(
                      account: result.account,
                      username: _username!,
                      signature: result.signature,
                    );
                    _userBloc.dispatch(signUpAction);

                    var failure = await signUpAction.result;
                    if (failure == null) {
                      Navigator.of(this.context).pop();
                    }
                  }
                },
              ),
            ],
    );
  }
}
