import 'package:flutter/material.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';

import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../widget_extensions.dart';

class UnlockWalletDialog extends StatefulWidget {
  const UnlockWalletDialog({super.key});

  @override
  State<UnlockWalletDialog> createState() => _UnlockWalletDialogState();
}

class _UnlockWalletDialogState extends StateX<UnlockWalletDialog> {
  late final _userBloc = use<UserBloc>();

  final _passwordController = TextEditingController();
  final _buttonController = RoundedLoadingButtonController();

  @override
  void dispose() {
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget buildX(BuildContext context) {
    return AlertDialog(
      backgroundColor: const Color(0xFF242423),
      title: Text(
        'Unlock the wallet',
        style: TextStyle(
          color: Colors.white,
        ),
      ),
      content: SizedBox(
        width: 300,
        height: 100,
        child: TextField(
          controller: _passwordController,
          obscureText: true,
          style: TextStyle(
            color: Colors.white,
          ),
          decoration: const InputDecoration(
            hintText: 'Password',
            hintStyle: TextStyle(color: Colors.white70),
            enabledBorder: UnderlineInputBorder(
              borderSide: BorderSide(color: Colors.white70),
            ),
            focusedBorder: UnderlineInputBorder(
              borderSide: BorderSide(color: Colors.white),
            ),
          ),
        ),
      ),
      actions: [
        RoundedLoadingButton(
          controller: _buttonController,
          color: Colors.white,
          valueColor: Colors.black,
          successColor: Colors.white,
          child: Text(
            'Unlock',
            style: TextStyle(
              color: Colors.black,
            ),
          ),
          onPressed: () async {
            var action = UnlockWallet(password: _passwordController.text);
            _userBloc.dispatch(action);

            var success = await action.result;
            if (success != null) {
              _buttonController.success();
            } else {
              _buttonController.error();
            }

            await Future.delayed(Duration(seconds: 1));
            if (context.mounted) {
              Navigator.of(context).pop(success != null);
            }
          },
        ),
      ],
    );
  }
}
