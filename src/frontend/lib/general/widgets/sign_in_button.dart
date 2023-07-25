import 'package:flutter/material.dart';

import '../../user/bloc/user_bloc.dart';
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

              await multiStageOffChainAction(
                context,
                (ctx) => _userBloc.signInWithEthereum(ctx),
              );

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
