import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:rounded_loading_button/rounded_loading_button.dart';

import '../utils/utils.dart';
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
        style: GoogleFonts.philosopher(
          color: Colors.white,
        ),
      ),
      content: SizedBox(
        width: 300,
        height: 50,
        child: TextField(
          controller: _passwordController,
          obscureText: true,
          style: const TextStyle(
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
          onSubmitted: (_) async {
            if (_buttonController.currentState != ButtonState.idle) return;
            _buttonController.start();
            await Future.delayed(const Duration(milliseconds: 500));

            var unlocked = await _userBloc.execute<bool>(
              UnlockWallet(password: _passwordController.text),
            );

            if (unlocked.isTrue) {
              _buttonController.success();
            } else {
              _buttonController.error();
            }

            // @@??: Why the modal gets automatically closed on <Enter> ?
            // await Future.delayed(const Duration(seconds: 1));
            // if (context.mounted) Navigator.of(context).pop(unlocked);
          },
        ),
      ),
      actions: [
        Padding(
          padding: const EdgeInsets.only(bottom: 6),
          child: RoundedLoadingButton(
            controller: _buttonController,
            color: Colors.white,
            valueColor: Colors.black,
            successColor: Colors.white,
            child: const Text(
              'Unlock',
              style: TextStyle(
                color: Colors.black,
              ),
            ),
            onPressed: () async {
              var unlocked = await _userBloc.execute<bool>(
                UnlockWallet(password: _passwordController.text),
              );

              if (unlocked.isTrue) {
                _buttonController.success();
              } else {
                _buttonController.error();
              }

              await Future.delayed(const Duration(seconds: 1));
              if (context.mounted) Navigator.of(context).pop(unlocked);
            },
          ),
        ),
      ],
    );
  }
}
