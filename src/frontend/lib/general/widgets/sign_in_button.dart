import 'package:flutter/material.dart';

import '../../user/bloc/user_actions.dart';
import '../../user/bloc/user_bloc.dart';
import '../../user/errors/wallet_locked_error.dart';
import '../../widget_extensions.dart';
import '../utils/utils.dart';

class SignInButton extends StatefulWidget {
  const SignInButton({super.key});

  @override
  State<SignInButton> createState() => _SignInButtonState();
}

class _SignInButtonState extends StateX<SignInButton> {
  late final _userBloc = use<UserBloc>();

  bool _inProgress = false;

  @override
  Widget buildX(BuildContext context) {
    return !_inProgress
        ? IconButton(
            icon: const Icon(
              Icons.door_sliding,
              color: Colors.white,
            ),
            onPressed: () async {
              setState(() => _inProgress = true);

              var action = SignInWithEthereum();
              _userBloc.dispatch(action);

              var failure = await action.result;
              if (failure != null && failure.error is WalletLockedError) {
                if (context.mounted) {
                  var unlocked = await showUnlockWalletDialog(context);
                  if (unlocked) {
                    _userBloc.dispatch(action);
                    await action.result;
                  }
                }
              }

              setState(() => _inProgress = false);
            },
          )
        : Container(
            width: 26,
            height: 16,
            padding: const EdgeInsets.only(right: 10),
            child: const CircularProgressIndicator(color: Colors.white),
          );
  }
}
